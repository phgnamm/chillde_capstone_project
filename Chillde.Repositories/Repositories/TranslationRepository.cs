using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.RequestModels;
using Microsoft.EntityFrameworkCore;

namespace Chillde.Repositories.Repositories
{
    public class TranslationRepository : GenericRepository<Translation>, ITranslationRepository
    {
        private readonly AppDbContext _context;

        public TranslationRepository(AppDbContext context, IClaimService claimService)
            : base(context, claimService)
        {
            _context = context;
        }

        public async Task<Translation?> GetTranslationAsync(string entityType, Guid entityId, string fieldName, Guid languageId)
        {
            return await _context.Translations
                .FirstOrDefaultAsync(t =>
                    t.EntityType == entityType &&
                    t.EntityId == entityId &&
                    t.FieldName == fieldName &&
                    t.LanguageId == languageId &&
                    !t.IsDeleted);
        }

        public async Task<Guid?> GetLanguageIdByCodeAsync(string languageCode)
        {
            if (Enum.TryParse(languageCode, true, out LanguageCode parsedCode))
            {
                var language = await _context.Languages
                    .FirstOrDefaultAsync(l => l.Code == parsedCode && !l.IsDeleted);

                return language?.Id;
            }
            return null;
        }

        public async Task<bool> DeleteTranslationsAsync(string entityType, Guid entityId)
        {
            var translations = await _context.Translations
                .Where(t => t.EntityType == entityType && t.EntityId == entityId)
                .ToListAsync();

            if (!translations.Any()) return false;

            foreach (var translation in translations)
            {
                SoftRemove(translation);
            }

            return true;
        }
        public async Task<IEnumerable<TModel>> GetEntitiesWithTranslationsAsync<TEntity, TModel>(
         List<Guid> entityIds,
         string sourceLanguageCode,
         Func<TEntity, TModel> mapEntityToModel,
         string? relationshipToInclude,
         string[] fieldsToTranslate,
         string[]? nestedRelationships = null,
         params string[] relationshipfields)
         where TEntity : BaseEntity
        {
            if (!Enum.TryParse(sourceLanguageCode, true, out LanguageCode languageCode))
            {
                throw new ArgumentException($"Invalid language code: {sourceLanguageCode}", nameof(sourceLanguageCode));
            }

            foreach (var field in fieldsToTranslate)
            {
                if (typeof(TModel).GetProperty(field) == null)
                {
                    throw new ArgumentException($"Field '{field}' does not exist in model '{typeof(TModel).Name}'", nameof(fieldsToTranslate));
                }
            }

            var entityIdsForRequest = entityIds;
            var entityIdsForRelationship = new List<Guid>();

            if (!string.IsNullOrEmpty(relationshipToInclude))
            {
                var relationshipProperty = typeof(TEntity).GetProperty(relationshipToInclude);
                if (relationshipProperty != null)
                {
                    var relationshipEntities = _context.Set<TEntity>()
                        .Where(e => entityIds.Contains(e.Id))
                        .Select(e => relationshipProperty.GetValue(e) as IEnumerable<BaseEntity>)
                        .ToList();

                    foreach (var relationshipEntityList in relationshipEntities)
                    {
                        if (relationshipEntityList != null)
                        {
                            foreach (var relationshipEntity in relationshipEntityList)
                            {
                                entityIdsForRelationship.Add(relationshipEntity.Id);
                            }
                        }
                    }
                }
            }

            var translationsQuery = _context.Translations
                .Where(t => (entityIdsForRequest.Contains(t.EntityId) || entityIdsForRelationship.Contains(t.EntityId))
                            && (fieldsToTranslate.Contains(t.FieldName) || relationshipfields.Contains(t.FieldName))
                            && t.Language.Code == languageCode)
                .GroupBy(t => t.EntityId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.ToDictionary(t => t.FieldName, t => t.TranslationText)
                );

            var translations = await translationsQuery;
            var entitiesQuery = _context.Set<TEntity>().Where(e => entityIds.Contains(e.Id));

            if (!string.IsNullOrEmpty(relationshipToInclude))
            {
                entitiesQuery = entitiesQuery.Include(relationshipToInclude);
            }

            var entities = await entitiesQuery.ToListAsync();

            var result = entities.Select(entity =>
            {
                var model = mapEntityToModel(entity);
                if (translations.TryGetValue(entity.Id, out var entityTranslations))
                {
                    foreach (var field in fieldsToTranslate)
                    {
                        var property = typeof(TModel).GetProperty(field);
                        if (property != null && entityTranslations.TryGetValue(field, out var translation))
                        {
                            property.SetValue(model, translation);
                        }
                    }
                }
                if (relationshipToInclude != null)
                {
                    var relationshipProperty = entity.GetType().GetProperty(relationshipToInclude);
                    if (relationshipProperty != null)
                    {
                        var relationshipValue = relationshipProperty.GetValue(entity) as IEnumerable<BaseEntity>;
                        if (relationshipValue != null)
                        {
                            var translatedDetails = relationshipValue.Select(detail =>
                            {
                                if (translations.TryGetValue(detail.Id, out var detailTranslations))
                                {
                                    foreach (var field in relationshipfields)
                                    {
                                        var property = detail.GetType().GetProperty(field);
                                        if (property != null && detailTranslations.TryGetValue(field, out var translation))
                                        {
                                            property.SetValue(detail, translation);
                                        }
                                    }
                                }
                                var requestDetail = detail as RequestDetail;
                                return new RequestDetailGetByIdModel
                                {
                                    Id = requestDetail!.Id,
                                    Description = requestDetail.Description,                                   
                                };
                            }).ToList();

                            var modelProperty = typeof(TModel).GetProperty(relationshipToInclude) ??
                                              typeof(TModel).GetProperty("RequestDetailGetByIdModels");

                            if (modelProperty != null && modelProperty.CanWrite)
                            {
                                modelProperty.SetValue(model, translatedDetails);
                            }
                        }
                    }
                }
                return model;
            });
            return result;
        }
    }
}
