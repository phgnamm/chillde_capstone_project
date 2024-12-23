using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Repositories
{
    public class OfferRepository : GenericRepository<Offer>, IOfferRepository
    {
        private readonly AppDbContext _context;
        private readonly IStringLocalizer<OfferRepository> _localizer;
        public OfferRepository(AppDbContext context, IClaimService claimService, IStringLocalizer<OfferRepository> localizer) : base(context, claimService)
        {
            _context = context;
            _localizer = localizer;
        }
        public async Task<IEnumerable<object>> GetOffersWithTranslationsAsync(string targetLanguageCode, List<Guid> offerIds)
        {
            var result = await _context.Offers
                .Where(o => offerIds.Contains(o.Id))
                .Join(
                        _context.Translations,
                        offer => offer.Id,
                        translation => translation.EntityId,
                        (offer, translation) => new { Offer = offer, Translation = translation }
                    )
                    .Where(joined => joined.Translation.FieldName == "Message" && joined.Translation.EntityType == "Offer"
                                     && joined.Translation.Language.Code.ToString() == targetLanguageCode)
                    .Select(joined => new
                    {
                        joined.Offer.Id,
                        Status = _localizer[$"OfferStatus.{joined.Offer.Status.ToString()}"],
                        Message = string.IsNullOrEmpty(joined.Translation.TranslationText) ? joined.Offer.Message : joined.Translation.TranslationText,
                        joined.Offer.RequestId,
                        joined.Offer.ServiceId,
                        joined.Offer.CreatedById,
                        joined.Offer.CreationDate
                    })
                .ToListAsync();

            return result;
        }
    }

}
