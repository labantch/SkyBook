using SkyBook.Business.ViewModels;

namespace SkyBook.Business.Interfaces;

public interface ISeatService
{

    Task<List<SeatSelectionVM>> GetSeatsByFlightAsync(int flightid);
    Task<bool> IsSeatAvailableAsync(int seatid, int flightid);
}