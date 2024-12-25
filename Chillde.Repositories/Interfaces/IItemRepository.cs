using Chillde.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Interfaces
{
    public interface IItemRepository : IGenericRepository<Item>
    {
        Task<Item?> GetFirstOrDefaultAsync(Expression<Func<Item, bool>> predicate);
       
    }
}
