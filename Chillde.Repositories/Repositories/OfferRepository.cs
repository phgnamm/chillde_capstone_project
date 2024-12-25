using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.OfferModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;


namespace Chillde.Repositories.Repositories
{
    public class OfferRepository : GenericRepository<Offer>, IOfferRepository
    {
        private readonly AppDbContext _context;
        public OfferRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
            _context = context;
        }
        public async Task<IEnumerable<OfferModel>> GetOffersWithTranslationsAsync(string sourceLanguageCode, List<Guid> offerIds)
        {
            if (!Enum.TryParse(sourceLanguageCode, true, out LanguageCode languageCode))
            {
            }
            var result = await _context.Offers
                .Where(o => offerIds.Contains(o.Id))
                .Join(
                    _context.Translations,
                    offer => offer.Id,
                    translation => translation.EntityId,
                    (offer, translation) => new { Offer = offer, Translation = translation }
                )
                .Where(joined => joined.Translation.FieldName == "Message" && joined.Translation.EntityType == "Offer"
                                 && joined.Translation.Language.Code == languageCode)
                .Select(joined => new OfferModel
                {
                    Id = joined.Offer.Id,
                    Status = joined.Offer.Status,
                    Message = string.IsNullOrEmpty(joined.Translation.TranslationText) ? joined.Offer.Message : joined.Translation.TranslationText,
                    RequestId = joined.Offer.RequestId,
                    ServiceId = joined.Offer.ServiceId
                })
                .ToListAsync();

            return result;
        }

        public async Task<OfferModel?> GetOfferAsync(Guid id, string targetLanguageCode)
        {
            var query = from offer in _context.Offers
                        where offer.Id == id
                        join translation in _context.Translations
                            on offer.Id equals translation.EntityId into translationGroup
                        from translation in translationGroup.DefaultIfEmpty()
                        select new OfferModel
                        {
                            Id = offer.Id,
                            Status = offer.Status,
                            Message = translation != null ? translation.TranslationText : offer.Message,
                            RequestId = offer.RequestId,
                            ServiceId = offer.ServiceId,
                            CreatedById = offer.CreatedById,
                            CreationDate = offer.CreationDate
                        };

            return await query.FirstOrDefaultAsync();
        }

    }

}
