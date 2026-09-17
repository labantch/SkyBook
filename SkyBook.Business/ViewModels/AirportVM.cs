using System.ComponentModel.DataAnnotations;
using SkyBook.Data.Models;

namespace SkyBook.Business.ViewModels //hola
{
    public class AirportVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(10, ErrorMessage = "Airport code cannot exceed 10 characters.")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Display(Name = "Station Operational Status")]
        public AirportStatus Status { get; set; } = AirportStatus.Active;
    }
}
