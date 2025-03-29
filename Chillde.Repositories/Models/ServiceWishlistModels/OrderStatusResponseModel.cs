using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Repositories.Models.ServiceWishlistModels
{
    //public class BaseResponseModel
    //{
    //    [JsonProperty("alias")]
    //    public bool Success { get; set; }
    //    [JsonProperty("alias")]
    //    public bool Success { get; set; }
    //    [JsonProperty("alias")]
    //    public bool Success { get; set; }

    //}
    public class OrderStatusResponseModel
    {
        [JsonProperty("label_id")]
        public string? LabelId { get; set; }

        [JsonProperty("partner_id")]
        public string? PartnerId { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("status_text")]
        public string? StatusText { get; set; }

        [JsonProperty("created")]
        public string? Created { get; set; }

        [JsonProperty("modified")]
        public string? Modified { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("pick_date")]
        public string? PickDate { get; set; }

        [JsonProperty("deliver_date")]
        public string? DeliverDate { get; set; }

        [JsonProperty("customer_fullname")]
        public string? CustomerFullname { get; set; }

        [JsonProperty("customer_tel")]
        public string? CustomerTel { get; set; }

        [JsonProperty("address")]
        public string? Address { get; set; }

        [JsonProperty("storage_day")]
        public int? StorageDay { get; set; }

        [JsonProperty("ship_money")]
        public int? ShipMoney { get; set; }

        [JsonProperty("insurance")]
        public int? Insurance { get; set; }

        [JsonProperty("value")]
        public int? Value { get; set; }

        [JsonProperty("weight")]
        public int? Weight { get; set; }

        [JsonProperty("pick_money")]
        public int? PickMoney { get; set; }

        [JsonProperty("is_freeship")]
        public int? IsFreeShip { get; set; }

        [JsonProperty("CreateLog")]
        public StatusHistory? StatusHistory { get; set; }
    }

    public class ShipmentStatusResponseModel
    {
        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("data")]
        public Packakge? Data { get; set; }
    }

    public class Packakge
    {
        [JsonProperty("Package")]
        public ShipmentData? Package { get; set; }
        [JsonProperty("CreateLog")]
        public List<StatusHistory>? StatusHistory { get; set; }

    }

    public class ShipmentData
    {
        [JsonProperty("alias")]
        public string? LabelId { get; set; }

        [JsonProperty("created")]
        public string? Created { get; set; }

        [JsonProperty("approved_at")]
        public string? Modified { get; set; }

        [JsonProperty("date_to_delay_pick")]
        public string? PickDate { get; set; }

        [JsonProperty("date_to_delay_deliver")]
        public string? DeliverDate { get; set; }

        [JsonProperty("customer_fullname")]
        public string? CustomerFullname { get; set; }

        [JsonProperty("customer_tel")]
        public string? CustomerTel { get; set; }

        [JsonProperty("customer_last_address")]
        public string? Address { get; set; }

        [JsonProperty("ship_money")]
        public int? ShipMoney { get; set; }

        [JsonProperty("insurance")]
        public int? Insurance { get; set; }

        [JsonProperty("value")]
        public int? Value { get; set; }

        [JsonProperty("weight")]
        public double? Weight { get; set; }

        [JsonProperty("pick_money")]
        public int? PickMoney { get; set; }

        [JsonProperty("is_freeship")]
        public int? IsFreeShip { get; set; }
    }

    public class StatusHistory
    {
        [JsonProperty("desc")]
        public string? Desc { get; set; }
        [JsonProperty("created")]
        public DateTime? Created { get; set; }
    }

    public class GHTKAccountInfo
    {
        [JsonProperty("data")]
        public GHTKData? Data { get; set; }
    }

    public class GHTKData
    {
        [JsonProperty("jwt")]
        public string? Jwt { get; set; }
    }

}
