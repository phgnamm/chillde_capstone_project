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
using Chillde.Repositories.Models.FeatureModels;
using Chillde.Repositories.Models.PackageModels;


namespace Chillde.Repositories.Repositories
{
    public class OfferRepository : GenericRepository<Offer>, IOfferRepository
    {
        private readonly AppDbContext _context;
        public OfferRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
        {
            _context = context;
        }

        public async Task<List<Offer>> GetOffersWithTranslationsAsync(string sourceLanguageCode, List<Guid> offerIds)
        {
            if (!Enum.TryParse(sourceLanguageCode, true, out LanguageCode languageCode))
                languageCode = LanguageCode.en;

            var offers = await _context.Offers
                .Include(o => o.OfferAttachments)
                .Include(o => o.CreatedBy)
                .Include(o => o.Package)
                    .ThenInclude(p => p.PackageFeatures)
                        .ThenInclude(pf => pf.Feature)
                .Where(o => offerIds.Contains(o.Id))
                .ToListAsync();

            var packageIds = offers.Select(o => o.Package!.Id).ToList();
            var featureIds = offers.SelectMany(o => o.Package!.PackageFeatures).Select(pf => pf.Feature.Id).Distinct().ToList();
            var packageFeatureIds = offers.SelectMany(o => o.Package!.PackageFeatures).Select(pf => pf.Id).Distinct().ToList();

            var translations = await _context.Translations
                .Where(t =>
                    t.Language.Code == languageCode &&
                    (
                        (t.EntityType == "Offer" && offerIds.Contains(t.EntityId)) ||
                        (t.EntityType == "Package" && packageIds.Contains(t.EntityId)) ||
                        (t.EntityType == "Feature" && featureIds.Contains(t.EntityId)) ||
                        (t.EntityType == "PackageFeature" && packageFeatureIds.Contains(t.EntityId))
                    )
                )
                .ToListAsync();

            var translationDict = translations
                .GroupBy(t => (t.EntityType, t.EntityId, t.FieldName))
                .ToDictionary(
                    g => g.Key,
                    g => g.First().TranslationText
                );

            foreach (var offer in offers)
            {
                offer.Message = GetTranslation(translationDict, "Offer", offer.Id, "Message", offer.Message!);

                var package = offer.Package!;
                package.Description = GetTranslation(translationDict, "Package", package.Id, "Description", package.Description!);

                foreach (var pf in package.PackageFeatures)
                {
                    var feature = pf.Feature;

                    feature.Name = GetTranslation(translationDict, "Feature", feature.Id, "Name", feature.Name!);
                    feature.Question = GetTranslation(translationDict, "Feature", feature.Id, "Question", feature.Question);
                    pf.Name = GetTranslation(translationDict, "PackageFeature", pf.Id, "Name", pf.Name!);
                }
            }

            return offers;
        }

        private static string GetTranslation(
            Dictionary<(string EntityType, Guid EntityId, string FieldName), string> translations,
            string entityType,
            Guid entityId,
            string fieldName,
            string fallback
        )
        {
            return translations.TryGetValue((entityType, entityId, fieldName), out var translated)
                ? translated
                : fallback;
        }


        public async Task<Offer> GetOfferAsync(Guid id, string sourceLanguageCode)
        {
            if (!Enum.TryParse(sourceLanguageCode, true, out LanguageCode languageCode))
                languageCode = LanguageCode.en;

            var offer = await _context.Offers
                 .Include(o => o.OfferAttachments)
                .Include(o => o.CreatedBy)
                .Include(o => o.Package)
                    .ThenInclude(p => p.PackageFeatures)
                        .ThenInclude(pf => pf.Feature)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (offer == null)
                return null;

            var packageId = offer.Package?.Id;
            var featureIds = offer.Package?.PackageFeatures.Select(pf => pf.Feature.Id).Distinct().ToList() ?? new();
            var packageFeatureIds = offer.Package?.PackageFeatures.Select(pf => pf.Id).Distinct().ToList() ?? new();

            var translations = await _context.Translations
                .Where(t =>
                    t.Language.Code == languageCode &&
                    (
                        (t.EntityType == "Offer" && t.EntityId == offer.Id) ||
                        (t.EntityType == "Package" && t.EntityId == packageId) ||
                        (t.EntityType == "Feature" && featureIds.Contains(t.EntityId)) ||
                        (t.EntityType == "PackageFeature" && packageFeatureIds.Contains(t.EntityId))
                    )
                )
                .ToListAsync();

            var translationDict = translations
                .GroupBy(t => (t.EntityType, t.EntityId, t.FieldName))
                .ToDictionary(
                    g => g.Key,
                    g => g.First().TranslationText
                );

            string Translate(string entityType, Guid entityId, string fieldName, string fallback)
            {
                return translationDict.TryGetValue((entityType, entityId, fieldName), out var text) ? text : fallback;
            }

            offer.Message = Translate("Offer", offer.Id, "Message", offer.Message ?? "");

            if (offer.Package != null)
            {
                offer.Package.Description = Translate("Package", offer.Package.Id, "Description", offer.Package.Description ?? "");

                foreach (var pf in offer.Package.PackageFeatures)
                {
                    var feature = pf.Feature;
                    feature.Name = Translate("Feature", feature.Id, "Name", feature.Name ?? "");
                    feature.Question = Translate("Feature", feature.Id, "Question", feature.Question ?? "");
                    pf.Name = Translate("PackageFeature", pf.Id, "Name", pf.Name ?? "");
                }
            }
            return offer;
        }


        public async Task<bool> RequestHasOffered(Guid id)
        {
            return await _context.Offers.AnyAsync(_ => _.ServiceId == id && _.Status == OfferStatus.Approved);
        }
    }

}
