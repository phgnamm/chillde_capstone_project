using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;
using Chillde.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public async Task<Shipment?> GetByTrackingIdAsync(string lableId)
    {
        return await _dbSet.FirstOrDefaultAsync(s=> s.Label == lableId);
    }

    public async Task<Shipment?> GetByOrderIdAsync(Guid orderId)
    {
        return await _dbSet
            .Include(s => s.ProductShipments)
            .FirstOrDefaultAsync(s => s.OrderId == orderId);
    }
}