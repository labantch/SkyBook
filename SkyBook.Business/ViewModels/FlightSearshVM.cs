using System;
using System.ComponentModel.DataAnnotations;

namespace SkyBook.Business.ViewModels
{
    public class FlightSearshVM
    {
        [Required]
        public int DepartureAirportId { get; set; }
        public int DepartureAirPortId { get => DepartureAirportId; set => DepartureAirportId = value; }

        [Required]
        public int ArrivalAirportId { get; set; }
        public int ArrivalAirPortId { get => ArrivalAirportId; set => ArrivalAirportId = value; }

        [Required]
        public DateTime TravelDate { get; set; }

        [Range(1, 10, ErrorMessage = "Passenger count must be between 1 and 10.")]
        public int PassengerCount { get; set; } = 1;
    }
}
