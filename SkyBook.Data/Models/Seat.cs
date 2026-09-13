using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBook.Data.Models
{
    public enum SeatClass
    {
        Economy = 1,
        Business = 2,
        FirstClass = 3
    }
    public class Seat
    {
        public int Id { get; set; }

        public string SeatNumber { get; set; }

        public SeatClass Class { get; set; }

        public int AircraftId { get; set; }

        public Aircraft Aircraft { get; set; }

        public ICollection<Booking> Bookings { get; set; }
    }
}
