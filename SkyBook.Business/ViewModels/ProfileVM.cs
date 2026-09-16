using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBook.Business.ViewModels
{
    public class ProfileVM
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Country {  get; set; }
        public string ImageUrl {  get; set; }
        public string? PhoneNumber {  get; set; }

    }
}
