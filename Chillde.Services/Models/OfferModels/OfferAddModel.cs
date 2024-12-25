using Chillde.Repositories.Enums;
using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.OfferModels
{
    public class OfferAddModel
    {
        [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters.")]
        public string? Message { get; set; } 

        [Required(ErrorMessage = "ServiceId is required.")]
        public Guid ServiceId { get; set; }
    }
}
