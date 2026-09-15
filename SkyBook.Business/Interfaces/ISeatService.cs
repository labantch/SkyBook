using SkyBook.Business.ViewModels;

namespace SkyBook.Business.Interfaces;

public interface ISeatService
{

  public  Task<List<SeatSelectionVM>> GetSeatsByFlightAsync(int flightid);
   public Task<bool> IsSeatAvailableAsync(int seatid, int flightid);
}