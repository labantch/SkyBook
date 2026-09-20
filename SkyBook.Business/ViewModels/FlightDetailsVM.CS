using SkyBook.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace SkyBook.Business.ViewModels
{
    public class FlightDetailsVM
    {
       public int FlightId { get; set; }
        public string FlightNumber { get; set; }
        public String AircraftName {  get; set; } 
        public string DepartureAirport{ get; set; }
        public string ArrivalAirport { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal Price { get; set; }
        public decimal EconomyPrice { get; set; }
        public decimal BusinessPrice { get; set; }
        public decimal FirstClassPrice { get; set; }
        public FlightStatus Status { get; set; }
        public int AvaliableSeats { get; set; }
        public List<SeatSelectionVM> Seats {  get; set; }
    }
}
