//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using SkyBook.Business.Interfaces;
//using SkyBook.Data.Data;
//using SkyBook.Data.Models;


//namespace SkyBook.Business.Service;

//public class ApplicationUserService : IApplicationUserService
//{

//    private ApplicationDbContext context = new ApplicationDbContext();
//    private readonly UserManager<ApplicationUser> userManager;
//    private readonly SignInManager<ApplicationUser> signInManager;
    
//    public ApplicationUserService(
//        UserManager<ApplicationUser> userManager,
//        SignInManager<ApplicationUser> signInManager)
//    {
//        this.userManager = userManager;
//        this.signInManager = signInManager;
//    }

//    public async Task<List<ApplicationUser>> GetAll()
//    {
//        var users = await context.ApplicationUsers.ToListAsync();
//        var list = new List<ApplicationUser>();
//        foreach (var user in users)
//        {
//            var vm = new ApplicationUser()
//            {
//                Id = user.Id,
//                FullName = user.FullName,
//                Country = user.Country,
//                Email = user.Email,
//                ImageUrl = user.ImageUrl
//            };
//            list.Add(vm);
//        }
//        return list;
//    }

//    public async Task<List<ApplicationUser>> GetById(int id)
//    {
//        var users = await context.ApplicationUsers.ToListAsync();
//        var list = new List<ApplicationUser>();
//        foreach (var user in users)
//        {
//            var vm = new ApplicationUser()
//            {
//                Id = user.Id,
//                FullName = user.FullName,
//                Country = user.Country,
//                Email = user.Email,
//                ImageUrl = user.ImageUrl,
//                PhoneNumber = user.PhoneNumber,
//                ConcurrencyStamp = user.ConcurrencyStamp
//            };
//            list.Add(vm);
//        }
//        return list;
//    }

//    public async Task Update(ApplicationUser user)
//    {
//        context.Update(user);
//        await context.SaveChangesAsync();
//    }

//    public async Task<bool> Login(string email, string password, bool remmberMe)
//    {
//        var user = await userManager.FindByEmailAsync(email);

//        if (user == null)
//            return false;

//        var result = await signInManager.PasswordSignInAsync(
//            user,
//            password,
//            remmberMe,
//            false);

//        return result.Succeeded;
        
//    }

//    public async Task<IdentityResult> Register(ApplicationUser user, string password)
//    {
//        return await userManager.CreateAsync(user, password);
//    }

//    public async Task Logout()
//    {
//        await signInManager.SignOutAsync();
//    }
//}