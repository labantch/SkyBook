using SkyBook.Business.ViewModels;
using SkyBook.Data.Models;

namespace SkyBook.Business.Interfaces;

public interface IFlightService
{
    public Task<List<FlightVM>> GetAllFlightsAsync();
    public Task<FlightDetailsVM> GetFlightsByIdAsync(int id);
    public Task CreatAsync(FlightVM flight);
    public Task EditAsync(FlightVM flight);
    public Task DeleteAsync(int id);
    public Task<FlightSearshVM> Search(int id);
    public Task<FlightVM> ChangeStatusAsync(int id, FlightStatus status);
    public Task<FlightCardVM> GetFlightCardAsync(int id);

}