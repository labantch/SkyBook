using SkyBook.Business.ViewModels;
using SkyBook.Data.Models;

namespace SkyBook.Business.Interfaces;

public interface IDashboardService
{
    public Task<DashboardVM> GetTotalUsersAsync();
    public Task<DashboardVM> GetTotalFlightsAsync();
    public Task<DashboardVM> GetTotalBookingsAsync();
    public Task<DashboardVM> GetTotalAircraftAsync();
    public Task<DashboardVM> GetAvailableSeatsAsync(int id);



}