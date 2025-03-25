using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;

namespace Chillde.Repositories.Repositories;

public class ShipmentRepository : GenericRepository<Shipment>, IShipmentRepository
{

    public ShipmentRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
    {
    }

    public bool HasAvalaibleShipment(Guid orderId, string partnerId)
    {
        var hasAvailableShipment =  _dbSet.Any(_ => _.OrderId == orderId && (_.PartnerId!.StartsWith(partnerId)));
        return hasAvailableShipment;
    }

    public Shipment? GetShipmentByPartnerIdOrLabel(string trackingOrder)
    {
        var shipment = _dbSet.Where(_ => _.PartnerId == trackingOrder || _.Label == trackingOrder).FirstOrDefault();
        return shipment;
    }
}