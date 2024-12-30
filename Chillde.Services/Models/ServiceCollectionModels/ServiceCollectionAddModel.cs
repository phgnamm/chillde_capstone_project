

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.ServiceCollectionModels
{
    public class ServiceCollectionAddModel 
    {
        [Required]
        public string? Name { get; set; }
        public IFormFile? ImageUrl { get; set; }
    }
}
