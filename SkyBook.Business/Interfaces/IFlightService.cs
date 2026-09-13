using SkyBook.Data.Models;

namespace SkyBook.Business.Interfaces;

public interface IFlightService
{
    // public FlightVM GetById(int id);
    public void Creat(Flight flight);
    public void Edit(Flight flight);
    public void Delete(int id);
    // public List<FlightVM> GetAll();
}