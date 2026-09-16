using Microsoft.AspNetCore.Identity;
using SkyBook.Data.Models;

namespace SkyBook.Business.Interfaces;

public interface IApplicationUserService
{
    public Task<bool> Login(string email, string password, bool remmberMe);
    public Task<IdentityResult> Register(ApplicationUser user, string password);
    public Task Logout();
}