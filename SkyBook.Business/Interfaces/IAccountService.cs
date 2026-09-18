using Microsoft.AspNetCore.Identity;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Models;

namespace SkyBook.Business.Interfaces;


   
    public interface IAccountService
{
      public Task<IdentityResult> RegisterAsync(RegisterVM model);
      public Task<SignInResult> LoginAsync(LoginVM model);
      public  Task LogoutAsync();
      //public  Task<ProfileVM?> GetProfileAsync(string userId);
      //public   Task<IdentityResult> UpdateProfileAsync(  string userId, ProfileVM model);
    }