using Microsoft.AspNetCore.Identity;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Models;

namespace SkyBook.Business.Service
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileService(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        #region GetProfile

        public async Task<ProfileVM?> GetProfileAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

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

        #endregion

        #region UpdateProfile

        public async Task<IdentityResult> UpdateProfileAsync(
            string userId,
            ProfileVM model)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "Invalid user."
                    });
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "User not found."
                    });
            }

            model.FullName = model.FullName?.Trim() ?? "";
            model.Email = model.Email?.Trim() ?? "";
            model.Country = model.Country?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "Full name is required."
                    });
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "Email is required."
                    });
            }

            user.FullName = model.FullName;
            user.Country = model.Country;
            user.PhoneNumber = model.PhoneNumber;
            user.ImageUrl = model.ImageUrl;

            if (!string.Equals(
                    user.Email,
                    model.Email,
                    StringComparison.OrdinalIgnoreCase))
            {
                var existingUser =
                    await _userManager.FindByEmailAsync(model.Email);

                if (existingUser != null &&
                    existingUser.Id != userId)
                {
                    return IdentityResult.Failed(
                        new IdentityError
                        {
                            Description = "This email is already used."
                        });
                }

                var emailResult =
                    await _userManager.SetEmailAsync(
                        user,
                        model.Email);

                if (!emailResult.Succeeded)
                    return emailResult;

                var userNameResult =
                    await _userManager.SetUserNameAsync(
                        user,
                        model.Email);

                if (!userNameResult.Succeeded)
                    return userNameResult;

                await _userManager.UpdateSecurityStampAsync(user);
            }

            return await _userManager.UpdateAsync(user);
        }

        #endregion

        #region ChangePassword
        public async Task<IdentityResult> ChangePasswordAsync(
            string userId,
            ChangePasswordVM model)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "Invalid user."
                    });
            }

            var user =
                await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "User not found."
                    });
            }

            if (model.NewPassword == model.CurrentPassword)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description =
                            "New password must be different from current password."
                    });
            }

            var result =
                await _userManager.ChangePasswordAsync(
                    user,
                    model.CurrentPassword,
                    model.NewPassword);

            if (!result.Succeeded)
                return result;

            await _userManager.UpdateSecurityStampAsync(user);

            return result;
        }

        #endregion
    }
}