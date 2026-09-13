using System.ComponentModel.DataAnnotations;

namespace SkyBook.Presentation.ViewModels
{
    public class AirportVM
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(10)]
        public string Code {  get; set; }

        [Required]
        [StringLength(100)]
        public  string City {  get; set; }

        [Required]
        [StringLength(100)]
        public  string Country {  get; set; }
    }
}
