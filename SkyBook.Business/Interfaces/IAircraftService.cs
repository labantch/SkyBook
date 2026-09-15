using SkyBook.Business.ViewModels;
using SkyBook.Data.Models;

namespace SkyBook.Business.Interfaces;

public interface IAircraftService
{
    Task<List<AircraftVM>> GetAllAircraftsAsync();
    Task<AircraftVM> GetAircraftByIdAsync(int id);
    Task CreateAircraftAsync(AircraftVM aircraft);
    Task UpdateAircraftAsync(AircraftVM aircraft);
    Task DeleteAircraftAsync(int id);

}