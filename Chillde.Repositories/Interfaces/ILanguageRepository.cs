using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Interfaces
{
    public interface ILanguageRepository : IGenericRepository<Language>
    {
        Task<Language?> GetByCodeAsync(LanguageCode code);
    }
}
