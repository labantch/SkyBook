using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Data;
using SkyBook.Data.Models;


namespace SkyBook.Business.Service;

public class AccountService : IAccountService

{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    public int MyProperty { get; set; }

    public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }
    #region Register
    public async Task<IdentityResult> RegisterAsync(RegisterVM model)
    {

        if (model == null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "NullModel",
                Description = "Model data cannot be null."
            });
        }


        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "DuplicateEmail",
                Description = "This email address is already registered."
            });
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            Country = model.Country,
            PhoneNumber = model.PhoneNumber,
            ImageUrl = model.ImageUrl
        };


        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return result;
        var roleName = "Customer";
        if (!await _roleManager.RoleExistsAsync(roleName))
        {

            await _roleManager.CreateAsync(new IdentityRole { Name = roleName });
        }
        var roleResult = await _userManager.AddToRoleAsync(user, roleName);

        if (!roleResult.Succeeded)
        {

            await _userManager.DeleteAsync(user);
            return roleResult;
        }
        return IdentityResult.Success;
    }
    #endregion
    #region Login
    public async Task<SignInResult> LoginAsync(LoginVM model)
    {
        if (model == null)
            return SignInResult.Failed;

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return SignInResult.Failed;
        return await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true
        );
    }
    #endregion

    #region Logout
    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
    #endregion

    #region GetProfile
    //public async Task<ProfileVM?> GetProfileAsync(string userId)
    //{
    //    if (string.IsNullOrEmpty(userId))
    //        return null;

    //    var user = await _userManager.FindByIdAsync(userId);

    //    if (user == null)
    //        return null;

    //    return new ProfileVM
    //    {
    //        FullName = user.FullName,
    //        Email = user.Email ?? string.Empty,
    //        Country = user.Country,
    //        PhoneNumber = user.PhoneNumber,
    //        ImageUrl = user.ImageUrl
    //    };
    //}
    #endregion

    #region UpdateProfile
    //public async Task<IdentityResult> UpdateProfileAsync(string userId, ProfileVM model)
    //{
    //    if (string.IsNullOrEmpty(userId) || model == null)
    //    {
    //        return IdentityResult.Failed(new IdentityError
    //        {
    //            Code = "InvalidData",
    //            Description = "Invalid user ID or model data."
    //        });
    //    }

    //    var user = await _userManager.FindByIdAsync(userId);

    //    if (user == null)
    //    {
    //        return IdentityResult.Failed(new IdentityError
    //        {
    //            Code = "UserNotFound",
    //            Description = "User not found."
    //        });
    //    }
    //    user.FullName = model.FullName;
    //    user.Country = model.Country;
    //    user.PhoneNumber = model.PhoneNumber;
    //    user.ImageUrl = model.ImageUrl;

    //    if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
    //    {
    //        var existingUser = await _userManager.FindByEmailAsync(model.Email);
    //        if (existingUser != null && existingUser.Id != userId)
    //        {
    //            return IdentityResult.Failed(new IdentityError
    //            {
    //                Code = "DuplicateEmail",
    //                Description = $"Email '{model.Email}' is already taken by another user."
    //            });
    //        }
    //        var emailResult = await _userManager.SetEmailAsync(user, model.Email);
    //        if (!emailResult.Succeeded)
    //            return emailResult;

    //        var usernameResult = await _userManager.SetUserNameAsync(user, model.Email);
    //        if (!usernameResult.Succeeded)
    //            return usernameResult;
    //        await _userManager.UpdateSecurityStampAsync(user);
    //    }
    //    return await _userManager.UpdateAsync(user);}


        #endregion
    }
