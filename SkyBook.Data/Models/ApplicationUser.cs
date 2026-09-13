using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace SkyBook.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }

        public string Country { get; set; }
        public string? ImageUrl {  get; set; }

        public ICollection<Booking> Bookings { get; set; }
    }
}
