using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SkyBook.Business.ViewModels
{
    public class ProfileVM
    {
 
            [Required]
            [StringLength(100)]
            public string FullName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [StringLength(256)]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(100)]
            public string Country { get; set; } = string.Empty;

            [StringLength(500)]
            public string? ImageUrl { get; set; }

            [Phone]
            [StringLength(50)]
            public string? PhoneNumber { get; set; }
        }
    }
