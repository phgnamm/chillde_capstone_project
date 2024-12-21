using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
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
    }
}
