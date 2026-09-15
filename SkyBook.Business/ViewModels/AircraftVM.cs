using System.ComponentModel.DataAnnotations;

namespace SkyBook.Business.ViewModels
{
    public class AircraftVM
    {
        public int ID { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        public int Capecity {  get; set; }
        public string? imageUrl { get; set; }
    }
}
