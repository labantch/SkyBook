using System.ComponentModel.DataAnnotations;

namespace SkyBook.Presentation.ViewModels
{
    public class LoginVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string RememberMe {  get; set; }
        

    }
}
