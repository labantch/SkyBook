using SkyBook.Data.Models;

namespace SkyBook.Business.ViewModels
{
    public class SeatSelectionVM
    {
        public int SeatId {  get; set; }
        public string SeatNumber {  get; set; }
        public SeatClass SeatClass { get; set; }
        public bool IsBooked {  get; set; }

    }
}
