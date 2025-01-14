using Chillde.Repositories.Enums;
using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.ServiceModels
{
    public class ServiceUpdateModel
    {
        [Required]
        public ServiceStatus Status { get; set; }
    }
}
