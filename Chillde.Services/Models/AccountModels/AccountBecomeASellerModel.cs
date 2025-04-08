using System.ComponentModel.DataAnnotations;
using Chillde.Services.Models.ShippingAddressModels;
using Microsoft.AspNetCore.Http;

namespace Chillde.Services.Models.AccountModels;

public class AccountBecomeASellerModel
{
    [Required] [StringLength(50)] public string FirstName { get; set; } = null!;
    [Required] [StringLength(50)] public string LastName { get; set; } = null!;
    // [Required] public string StoreAddress { get; set; } = null!;
    [Required] public ShippingAddressAddModel ShippingAddress { get; set; } = null!;
    [Required] public string StoreDescription { get; set; } = null!;
    public IFormFile? NewImage { get; set; }
    public IFormFile? NewBanner { get; set; }
}