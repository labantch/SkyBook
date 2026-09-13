using System.ComponentModel.DataAnnotations;

namespace SkyBook.Presentation.ViewModels
{
    public class AircraftVM
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        public string Capecity {  get; set; }
    }
}
