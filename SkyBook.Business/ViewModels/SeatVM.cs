using SkyBook.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace SkyBook.Business.ViewModels
{
    public class SeatVM
    {
        public int Id { get; set; }

        [Required]
        public int AircraftId { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Seat number cannot exceed 10 characters.")]
        public string SeatNumber { get; set; } = string.Empty;

        [Required]
        public SeatClass Class { get; set; }
    }
}
