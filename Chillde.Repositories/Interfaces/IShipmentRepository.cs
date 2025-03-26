using Chillde.Repositories.Entities;
using Chillde.Repositories.Enums;

namespace Chillde.Repositories.Interfaces;

public interface IShipmentRepository : IGenericRepository<Shipment>
{
    bool HasAvalaibleShipment(Guid orderId, string partnerId);
    Shipment? GetShipmentByPartnerIdOrLabel(string trackingOrder);

}