using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Chillde.Repositories.Models.OfferModels;
using Chillde.Repositories.Models.RequestModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Repositories
{
    public class RequestRepository : GenericRepository<Request>, IRequestRepository
    {
        private readonly AppDbContext _context;
        public RequestRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
            _context = context;
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
                            ServiceId = offer.ServiceId ?? Guid.Empty,
                            CreatedById = offer.CreatedById,
                            CreationDate = offer.CreationDate
                        };

            return await query.FirstOrDefaultAsync();
        }
    }
}

