using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Data;
using SkyBook.Data.Models;
using System;
using System.Threading.Tasks;

namespace SkyBook.Business.Service;

public class ApplicationUserService : IApplicationUserService
{
    private readonly ApplicationDbContext context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationUserService(
        ApplicationDbContext _context,
        UserManager<ApplicationUser> userManager)
    {
        context = _context;
        _userManager = userManager;
    }

    public async Task<LoginVM> LoginAsync(LoginVM model)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.PasswordHash == model.Password);

        if (user == null)
        {
            throw new Exception("Account Not Found, Please Check your Email or Password");
        }

        return model;
    }

    public Task<bool> Login(string email, string password, bool remmberMe)
    {
        throw new NotImplementedException();
    }

    public Task Logout()
    {
        throw new NotImplementedException();
    }

    public Task<IdentityResult> Register(ApplicationUser user, string password)
    {
        throw new NotImplementedException();
    }

    public Task<RegisterVM> RegisterAsync(RegisterVM model)
    {
        throw new NotImplementedException();
    }

    public Task LogoutAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<List<UserVM>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        var result = new List<UserVM>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserVM
            {
                Id = user.Id,
                FullName = user.FullName ?? user.UserName ?? "Unknown",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                Country = user.Country,
                ImageUrl = user.ImageUrl,
                Role = roles.Contains("Admin") ? "Admin" : "User"
            });
        }

        return result;
    }

    public async Task<UserVM?> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new UserVM
        {
            Id = user.Id,
            FullName = user.FullName ?? user.UserName ?? "Unknown",
            Email = user.Email ?? "",
            PhoneNumber = user.PhoneNumber,
            Country = user.Country,
            ImageUrl = user.ImageUrl,
            Role = roles.Contains("Admin") ? "Admin" : "User"
        };
    }

    public async Task<bool> PromoteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

       
        if (!await _userManager.IsInRoleAsync(user, "Admin"))
            await _userManager.AddToRoleAsync(user, "Admin");

        return true;
    }

    public async Task<bool> DemoteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        
        if (await _userManager.IsInRoleAsync(user, "Admin"))
            await _userManager.RemoveFromRoleAsync(user, "Admin");

        return true;
    }
}