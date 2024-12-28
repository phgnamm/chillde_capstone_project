using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

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
        params string[] fieldsToTranslate)
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
            var translations = await _context.Translations
                .Where(t => entityIds.Contains(t.EntityId)
                            && fieldsToTranslate.Contains(t.FieldName)
                            && t.Language.Code == languageCode)
                .GroupBy(t => t.EntityId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.ToDictionary(t => t.FieldName, t => t.TranslationText)
                );
            var entities = await _context.Set<TEntity>()
                .Where(e => entityIds.Contains(e.Id))
                .ToListAsync();
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

                return model;
            });

            return result;
        }

    }
}
