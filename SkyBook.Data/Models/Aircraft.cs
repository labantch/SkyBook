using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBook.Data.Models
{
    public class Aircraft
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        public string? ImageUrl { get; set; }
        public ICollection<Seat> Seats { get; set; }
        public ICollection<Flight> Flights { get; set; }
    }
}
