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

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        
        public async Task<IdentityResult> RegisterAsync(RegisterVM model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Country = model.Country,
                PhoneNumber = model.PhoneNumber,
                ImageUrl = model.ImageUrl
            };

            var result = await _userManager.CreateAsync( user, model.Password );

            if (!result.Succeeded)
                return result;

            var roleResult = await _userManager.AddToRoleAsync( user,"Customer");

            if (!roleResult.Succeeded)
                return roleResult;

            return result;
        }


     
        public async Task<SignInResult> LoginAsync(LoginVM model)
        {
            return await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true
            );
        }
      
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }


        public async Task<ProfileVM?> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return null;

            return new ProfileVM
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Country = user.Country,
                PhoneNumber = user.PhoneNumber,
                ImageUrl = user.ImageUrl
            };
        }


        public async Task<IdentityResult> UpdateProfileAsync(string userId,ProfileVM model)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                    {
                        Description = "User not found."
                    }
                );
            }

            user.FullName = model.FullName;
            user.Country = model.Country;
            user.PhoneNumber = model.PhoneNumber;
            user.ImageUrl = model.ImageUrl;

            
            if (user.Email != model.Email)
            {
                var emailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!emailResult.Succeeded)
                    return emailResult;
                var usernameResult = await _userManager.SetUserNameAsync(  user, model.Email );
                if (!usernameResult.Succeeded)
                    return usernameResult;
            }
        return await _userManager.UpdateAsync(user);
    }
}


    