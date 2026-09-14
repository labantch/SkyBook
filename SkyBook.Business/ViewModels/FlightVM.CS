using SkyBook.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace SkyBook.Business.ViewModels
{
    public class FlightVM
    {
        public int Id {  get; set; }
        [Required]
        [StringLength(20)]
        public string FlightNumber {  get; set; }
        [Required]
        public int DepartureAirportId {  get; set; }
        [Required]
        public int ArrivalAirportId {  get; set; }
        [Required]
        public int AircraftID {  get; set; }
        [Required]
        public DateTime DepartureTime {  get; set; }
        [Required]
        public DateTime ArrivalTime { get; set; }
        [Required]
        [Range(1,100000)]
        public decimal Price {  get; set; }
        public FlightStatus Status {  get; set; }


    }
}
