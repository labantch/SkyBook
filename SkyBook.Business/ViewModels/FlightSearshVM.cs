using System;
using System.ComponentModel.DataAnnotations;

namespace SkyBook.Business.ViewModels
{
    public class FlightSearshVM
    {
        [Required]
        public int DepartureAirportId { get; set; }
        [Required]
        public int ArrivalAirportId { get; set; }
        [Required]
        public DateTime TravelDate { get; set; }

        [Range(1, 10, ErrorMessage = "Passenger count must be between 1 and 10.")]
        public int PassengerCount { get; set; } = 1;
    }
}
