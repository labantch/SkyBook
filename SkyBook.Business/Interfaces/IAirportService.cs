using SkyBook.Data.Models;

namespace SkyBook.Business.Interfaces;

public interface IAirportService
{
    public void Add(Airport airport);
    public void Delete(int id);
    public void Edit(Airport airport);
    // public List<AirportViewModel> GetAll();
}