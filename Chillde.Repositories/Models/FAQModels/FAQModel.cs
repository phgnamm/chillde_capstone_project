using Chillde.Repositories.Entities;

namespace Chillde.Repositories.Models.FAQModels
{
    public class FAQModel : BaseEntity
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public Guid ServiceId { get; set; }
    }
}
