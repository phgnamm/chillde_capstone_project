using Chillde.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        public Task<Category?> GetFirstOrDefaultAsync(Expression<Func<Category, bool>> predicate);
    }
}
