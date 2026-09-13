using SkyBook.Data.Models;

namespace SkyBook.Business.Interfaces;

public interface IAircraftService
{
    public void Add(Aircraft aircraft);
    public void Delete(int id);
    public void Edit(Aircraft aircraft);
    // public List<AirportViewModel> GetAll();
}