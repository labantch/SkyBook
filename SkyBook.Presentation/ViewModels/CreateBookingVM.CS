using System.ComponentModel.DataAnnotations;

namespace SkyBook.Presentation.ViewModels
{
    public class CreateBookingVM
    {
        public int FlightId { get; set; }
        [Required]
        public int SeatID { get; set; }
        [Required]
        [StringLength(100)]
        public String FullName {  get; set; }
        [Required]
        [EmailAddress]
        public string Email {  get; set; }
        [Required]
        [Phone]
        public string PhoneNumber {  get; set; }
        [Required]
        [StringLength (100)]
        public string PassportNumber {  get; set; }
    }
}
