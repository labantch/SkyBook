using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SkyBook.Data.Models;

namespace SkyBook.Business.ViewModels
{
    public class AircraftVM : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Aircraft Model is required.")]
        [StringLength(100, ErrorMessage = "Aircraft Model cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Passenger Capacity is required.")]
        [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000.")]
        public int Capacity { get; set; }

        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Fleet Operational Status")]
        public AircraftStatus Status { get; set; } = AircraftStatus.Active;

        [Range(0, 1000, ErrorMessage = "Economy seats must be between 0 and 1000.")]
        [Display(Name = "Economy Seats")]
        public int EconomySeats { get; set; }

        [Range(0, 1000, ErrorMessage = "Business seats must be between 0 and 1000.")]
        [Display(Name = "Business Seats")]
        public int BusinessSeats { get; set; }

        [Range(0, 1000, ErrorMessage = "First Class seats must be between 0 and 1000.")]
        [Display(Name = "First Class Seats")]
        public int FirstClassSeats { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }
}
