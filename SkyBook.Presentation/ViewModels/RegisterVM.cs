using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace SkyBook.Presentation.ViewModels
{
    public class RegisterVM
    {
        [Required]
        [StringLength(100)]
        public String FullName {  get; set; }
        [Required]
        [EmailAddress]
        public string Email {  get; set; }
        [Required]
        [StringLength (100)]
        public string Country {  get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

    }
}
