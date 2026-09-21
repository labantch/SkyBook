using SkyBook.Business.ViewModels;
using SkyBook.Data.Models;

namespace SkyBook.Business.Interfaces;

public interface IAircraftService
{
   public Task<List<AircraftVM>> GetAllAircraftsAsync();
   public Task<AircraftVM?> GetAircraftByIdAsync(int id);
   public Task CreateAsync(AircraftVM aircraft);
   public Task UpdateAsync(AircraftVM aircraft);
   public Task DeleteAsync(int id);

}