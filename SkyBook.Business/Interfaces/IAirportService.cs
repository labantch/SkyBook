using SkyBook.Data.Models;
using System.Security.Principal;
using SkyBook.Business.ViewModels;


namespace SkyBook.Business.Interfaces;

public interface IAirportService
{
    public Task<List<AirportVM>> GetAllAirportAsync();
    public Task<AirportVM> GetAirportByIdAsync(int id);
    public Task CreateAsync(AirportVM airport);
    public Task UpdateAirportAsync(AirportVM airport);
    public Task DeleteAirportAsync(int id);
    
}