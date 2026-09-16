using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Data.Data;
using SkyBook.Data.Models;


namespace SkyBook.Business.Service;

public class ApplicationUserService : IApplicationUserService
{

    private ApplicationDbContext context = new ApplicationDbContext();

}