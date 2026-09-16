using System;
using System.ComponentModel.DataAnnotations;

namespace SkyBook.Business.ViewModels
{
    public class PassengerVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; } = string.Empty;
        [EmailAddress]
        public string? Email {  get; set; }
        [Phone]
        public string? Phone { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Passport number cannot exceed 20 characters.")]
        public string PassportNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "Nationality cannot exceed 50 characters.")]
        public string Nationality { get; set; } = string.Empty;
    }
}
