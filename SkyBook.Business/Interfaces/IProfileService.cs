using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Data;
using SkyBook.Data.Models;
namespace SkyBook.Business.Interfaces;

public interface IProfileService
{
    Task<ProfileVM> GetProfileAsync(string userId);
    Task<IdentityResult> UpdateProfileAsync(string userId, ProfileVM model);
    Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordVM model);
    



}
