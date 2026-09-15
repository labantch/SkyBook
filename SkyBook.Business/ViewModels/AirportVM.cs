using System.ComponentModel.DataAnnotations;

namespace SkyBook.Business.ViewModels
{
    public class AirportVM
    {
        public int Id { get; set; }
        public int id { get => Id; set => Id = value; }

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
    }
}
