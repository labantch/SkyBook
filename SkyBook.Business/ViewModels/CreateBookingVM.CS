using System;
using System.ComponentModel.DataAnnotations;

namespace SkyBook.Business.ViewModels
{
    public class CreateBookingVM
    {
        [Required]
        public int FlightId { get; set; }

        [Required]
        public int SeatId { get; set; }
        

        [Required]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        public string FullName
        {
            get => $"{FirstName} {LastName}".Trim();
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var parts = value.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    FirstName = parts[0];
                    LastName = parts.Length > 1 ? parts[1] : string.Empty;
                }
            }
        }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Nationality cannot exceed 50 characters.")]
        public string Nationality { get; set; } = string.Empty;

        [Required]
        [StringLength(20, ErrorMessage = "Passport number cannot exceed 20 characters.")]
        public string PassportNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
