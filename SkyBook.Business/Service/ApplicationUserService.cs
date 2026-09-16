using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Data;
using SkyBook.Data.Models;


namespace SkyBook.Business.Service;

public class ApplicationUserService : IApplicationUserService
{

    private readonly ApplicationDbContext context;
    

    public ApplicationUserService(ApplicationDbContext _context)
    {
        context = _context;
        
    }


    public async Task<LoginVM> LoginAsync(LoginVM model)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.PasswordHash == model.Password);
        
        if (user == null)
        {
            throw new Exception("Account Not Found, Please Check your Email or Password");
        }

        if (model.RememberMe == true)
        {
            model.RememberMe = true;
        }

        model.Email = user.Email;
        return model;


    }

    public async Task<RegisterVM> RegisterAsync(RegisterVM model)
    {
        var emailExist = await context.Users.AnyAsync(e => e.Email == model.Email);
        if (emailExist)
        {
            throw new Exception("Email Already Used");
        }

        var user = new ApplicationUser
        {
            FullName = model.FullName,
            Email = model.Email,
            Country = model.Country,
            PhoneNumber = model.PhoneNumber,
            ImageUrl = model.ImageUrl,
            PasswordHash = model.Password
            
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        return model;

    }

    public async Task LogoutAsync()
    {
        
    }
}