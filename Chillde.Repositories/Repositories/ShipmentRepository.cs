using Chillde.Repositories.Entities;
using Chillde.Repositories.Interfaces;

namespace Chillde.Repositories.Repositories;

public class ShipmentRepository : GenericRepository<Shipment>, IShipmentRepository
{

    public ShipmentRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
    {
    }

}