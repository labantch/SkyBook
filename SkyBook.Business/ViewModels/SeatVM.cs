using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;

namespace SkyBook.Business.ViewModel
{
    public class SeatVM
    {
     public int AircraftId {  get; set; }
        [Required]
        [StringLength(100)]
     public string SeatNumber {  get; set; }

    
    }
}
