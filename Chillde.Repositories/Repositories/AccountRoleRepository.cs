using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;

namespace Chillde.Repositories.Repositories;

public class AccountRoleRepository : GenericRepository<AccountRole>, IAccountRoleRepository
{
    public AccountRoleRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
    {
    }
}