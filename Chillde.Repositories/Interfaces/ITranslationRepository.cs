using Chillde.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Interfaces
{
    public interface ITranslationRepository : IGenericRepository<Translation>
    {
        Task<Translation?> GetTranslationAsync(string entityType, Guid entityId, string fieldName, Guid languageId);
        Task<Guid?> GetLanguageIdByCodeAsync(string languageCode);
        Task<IEnumerable<Translation>> GetTranslationsAsync(string entityType, Guid entityId, Guid languageId);
        Task<bool> DeleteTranslationsAsync(string entityType, Guid entityId);

    }
}
