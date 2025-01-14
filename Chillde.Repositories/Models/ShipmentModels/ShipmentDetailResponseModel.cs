using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.ShipmentModels
{
    public class ShipmentDetailResponseModel
    {
        public int ShopId { get; set; }
        public int ClientId { get; set; }
        public string ReturnName { get; set; }
        public string ReturnPhone { get; set; }
        public string ReturnAddress { get; set; }
        public string ReturnWardCode { get; set; }
        public int ReturnDistrictId { get; set; }
        public string FromName { get; set; }
        public string FromPhone { get; set; }
        public string FromAddress { get; set; }
        public string FromWardCode { get; set; }
        public int FromDistrictId { get; set; }
        public int DeliverStationId { get; set; }
        public string ToName { get; set; }
        public string ToPhone { get; set; }
        public string ToAddress { get; set; }
        public string ToWardCode { get; set; }
        public int ToDistrictId { get; set; }
        public int Weight { get; set; }
        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int ConvertedWeight { get; set; }
        public int ServiceTypeId { get; set; }
        public int ServiceId { get; set; }
        public int PaymentTypeId { get; set; }
        public decimal CustomServiceFee { get; set; }
        public decimal CodAmount { get; set; }
        public string CodCollectDate { get; set; }
        public string CodTransferDate { get; set; }
        public bool IsCodTransferred { get; set; }
        public bool IsCodCollected { get; set; }
        public decimal InsuranceValue { get; set; }
        public decimal OrderValue { get; set; }
        public int PickStationId { get; set; }
        public string ClientOrderCode { get; set; }
        public decimal CodFailedAmount { get; set; }
        public string CodFailedCollectDate { get; set; }
        public string RequiredNote { get; set; }
        public string Content { get; set; }
        public string Note { get; set; }
        public string EmployeeNote { get; set; }
        public string Coupon { get; set; }
        public string OrderCode { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public List<ShipmentLog> Log { get; set; }


    }
    public class ShipmentLog
    {
        public string Status { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
