using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.OfferModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Chillde.Repositories.Models.AccountModels;


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
                    Status = joined.Offer.Status.ToString(),
                    Message = string.IsNullOrEmpty(joined.Translation.TranslationText) ? joined.Offer.Message : joined.Translation.TranslationText,
                    MinWeight = joined.Offer.MinWeight,
                    MaxWeight = joined.Offer.MaxWeight,
                    OfferAttachments = joined.Offer.OfferAttachments.ToList(),
                    RequestId = joined.Offer.RequestId,
                    ServiceId = joined.Offer.ServiceId ?? Guid.Empty,
                    CreatedBy = new AccountLiteModel
                    {
                        Email = joined.Offer.CreatedBy.Email,
                        FirstName = joined.Offer.CreatedBy.FirstName,
                        LastName = joined.Offer.CreatedBy.LastName,
                        Image = joined.Offer.CreatedBy.Image,
                    },
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
                            Status = offer.Status.ToString(),
                            Message = translation != null ? translation.TranslationText : offer.Message,
                            MinWeight = offer.MinWeight,
                            MaxWeight = offer.MaxWeight,
                            OfferAttachments = offer.OfferAttachments.ToList(),
                            RequestId = offer.RequestId,
                            ServiceId = offer.ServiceId,
                            CreatedBy = new AccountLiteModel
                            {
                                Email = offer.CreatedBy.Email,
                                FirstName = offer.CreatedBy.FirstName,
                                LastName = offer.CreatedBy.LastName,
                                Image = offer.CreatedBy.Image
                            },
                            CreationDate = offer.CreationDate
                        };

            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> RequestHasOffered(Guid id)
        {
            return await _context.Offers.AnyAsync(_ => _.ServiceId == id && _.Status == OfferStatus.Approved);
        }
    }

}
