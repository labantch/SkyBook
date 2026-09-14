using System.ComponentModel.DataAnnotations;

namespace SkyBook.Business.ViewModels
{
    public class FlightSearshVM
    {
        [Required]
        public int DepartureAirPortId { get; set; }
        [Required]
        public int ArrivalAirPortId { get; set; }
        [Required]
        public DateTime TravelDate {  get; set; }
        [Range(1,10)]
        public int PassengerCount {  get; set; }

    }
}
