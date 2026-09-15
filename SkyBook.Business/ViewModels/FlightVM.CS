using SkyBook.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace SkyBook.Business.ViewModels
{
    public class FlightVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string FlightNumber { get; set; } = string.Empty;

        [Required]
        public int DepartureAirportId { get; set; }

        [Required]
        public int ArrivalAirportId { get; set; }

        [Required]
        public int AircraftId { get; set; }
        public int AircraftID { get => AircraftId; set => AircraftId = value; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        [Required]
        [Range(0.01, 100000.00)]
        public decimal Price { get; set; }

        public FlightStatus Status { get; set; }
    }
}
