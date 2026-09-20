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

        [Range(0.00, 100000.00)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Economy fare is required.")]
        [Range(0.01, 100000.00, ErrorMessage = "Economy fare must be greater than zero.")]
        public decimal EconomyPrice { get; set; }

        [Required(ErrorMessage = "Business fare is required.")]
        [Range(0.01, 100000.00, ErrorMessage = "Business fare must be greater than zero.")]
        public decimal BusinessPrice { get; set; }

        [Required(ErrorMessage = "First Class fare is required.")]
        [Range(0.01, 100000.00, ErrorMessage = "First Class fare must be greater than zero.")]
        public decimal FirstClassPrice { get; set; }

        public FlightStatus Status { get; set; }

        public string? DepartureAirportCode { get; set; }
        public string? DepartureAirportName { get; set; }
        public string? DepartureAirportCity { get; set; }

        public string? ArrivalAirportCode { get; set; }
        public string? ArrivalAirportName { get; set; }
        public string? ArrivalAirportCity { get; set; }

        public string? AircraftName { get; set; }

        public int AvailableSeats { get; set; }

        public List<FlightStopVM> Stops { get; set; } = new();
        public bool IsDirect => Stops == null || Stops.Count == 0;
    }
}
