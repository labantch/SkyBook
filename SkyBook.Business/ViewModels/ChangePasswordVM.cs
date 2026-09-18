using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SkyBook.Business.ViewModels
{
    public class ChangePasswordVM
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;
        [Required]
        [MinLength(8)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;
        [Required]
        [Compare("NewPassword")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }= string.Empty;

    }
}
