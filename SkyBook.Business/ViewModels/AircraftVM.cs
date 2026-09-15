using System.ComponentModel.DataAnnotations;

namespace SkyBook.Business.ViewModels
{
    public class AircraftVM
    {
        public int Id { get; set; }
        public int ID { get => Id; set => Id = value; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000.")]
        public int Capacity { get; set; }

        public int Capecity { get => Capacity; set => Capacity = value; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public string? imageUrl { get => ImageUrl; set => ImageUrl = value; }
    }
}
