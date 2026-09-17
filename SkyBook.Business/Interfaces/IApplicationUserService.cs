using Microsoft.AspNetCore.Identity;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Models;

namespace SkyBook.Business.Interfaces;

public interface IApplicationUserService
{
    public Task<LoginVM> LoginAsync(LoginVM model);
    public Task<RegisterVM> RegisterAsync(RegisterVM model);
    public Task LogoutAsync();
    public Task<List<UserVM>> GetAllUsersAsync();
    public Task<UserVM?> GetUserByIdAsync(string userId);
}