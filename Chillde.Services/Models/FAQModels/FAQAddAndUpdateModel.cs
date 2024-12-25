using System.ComponentModel.DataAnnotations;

namespace Chillde.Services.Models.FAQModels
{
    public class FAQAddAndUpdateModel
    {
        [Required(ErrorMessage = "Question is required.")]
        public string Question { get; set; }
        [Required(ErrorMessage = "Answer is required.")]
        public string Answer { get; set; }
    }
}
