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
        public int AircraftId {  get; set; }
        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        [Required]
        [Range(0.01, 100000.00)]
        public decimal Price { get; set; }

        public FlightStatus Status { get; set; }

        // Navigation Display Helpers
        public string? DepartureAirportCode { get; set; }
        public string? DepartureAirportName { get; set; }
        public string? DepartureAirportCity { get; set; }

        public string? ArrivalAirportCode { get; set; }
        public string? ArrivalAirportName { get; set; }
        public string? ArrivalAirportCity { get; set; }

        public string? AircraftName { get; set; }
    }
}
