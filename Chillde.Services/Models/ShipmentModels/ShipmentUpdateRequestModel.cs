using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Models.ShipmentModels
{
    public class ShipmentUpdateRequestModel
    {
        [FromForm(Name = "partner_id")]
        public string PartnerId { get; set; } = string.Empty;

        [FromForm(Name = "label_id")]
        public string LabelId { get; set; } = string.Empty;

        [FromForm(Name = "status_id")]
        public int StatusId { get; set; }

        [FromForm(Name = "action_time")]
        public string? ActionTime { get; set; } = string.Empty;

        [FromForm(Name = "reason_code")]
        public string? ReasonCode { get; set; }

        [FromForm(Name = "reason")]
        public string? Reason { get; set; }

        [FromForm(Name = "weight")]
        public decimal? Weight { get; set; }

        [FromForm(Name = "fee")]
        public decimal? Fee { get; set; }

        [FromForm(Name = "pick_money")]
        public decimal? PickMoney { get; set; }

        [FromForm(Name = "return_part_package")]
        public int? ReturnPartPackage { get; set; }
    }
}
