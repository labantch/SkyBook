using SkyBook.Business.ViewModels;
using SkyBook.Data.Models;

namespace SkyBook.Business.Interfaces;

public interface IAircraftService
{
   public Task<List<AircraftVM>> GetAllAircraftsAsync();
   public Task<AircraftVM> GetAircraftByIdAsync(int id);
   public Task CreateAircraftAsync(AircraftVM aircraft);
   public Task UpdateAircraftAsync(AircraftVM aircraft);
   public Task DeleteAircraftAsync(int id);

}