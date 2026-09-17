using SkyBook.Business.ViewModels;
using SkyBook.Data.Models;

namespace SkyBook.Business.Interfaces;

public interface IFlightService
{
      public  Task<List<FlightVM>> GetAllFlightsAsync();
      public  Task<FlightDetailsVM?> GetFlightByIdAsync(int id);
      public Task CreateFlightAsync(FlightVM model);
      public Task UpdateFlightAsync(FlightVM model);
      public Task DeleteFlightAsync(int id);
      public Task<List<FlightCardVM>> SearchFlightsAsync(FlightSearshVM flightSearshVM);
      public Task ChangeStatusAsync( int flightId, FlightStatus status);
    public Task<FlightVM>GetFlightForEditAsync(int id);
    


}