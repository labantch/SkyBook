using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkyBook.Business.Interfaces;
using SkyBook.Business.Service;
using SkyBook.Data.Data;
using SkyBook.Data.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<SkyBook.Data.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<SkyBook.Business.Interfaces.IAirportService, SkyBook.Business.Service.AirportService>();
builder.Services.AddScoped<SkyBook.Business.Interfaces.IAircraftService, SkyBook.Business.Service.AircraftService>();
builder.Services.AddScoped<SkyBook.Business.Interfaces.IFlightService, SkyBook.Business.Service.FlightService>();
builder.Services.AddScoped<SkyBook.Business.Interfaces.IAccountService, SkyBook.Business.Service.AccountService>();
builder.Services.AddScoped<SkyBook.Business.Interfaces.IBookingService, SkyBook.Business.Service.BookingService>();
builder.Services.AddScoped<SkyBook.Business.Interfaces.ISeatService, SkyBook.Business.Service.SeatService>();
builder.Services.AddScoped<SkyBook.Business.Interfaces.IDashboardService, SkyBook.Business.Service.DashboardService>();
builder.Services.AddScoped<SkyBook.Business.Interfaces.IApplicationUserService, SkyBook.Business.Service.ApplicationUserService>();
builder.Services.AddScoped<SkyBook.Business.Interfaces.IProfileService, SkyBook.Business.Service.ProfileService>();



var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();





app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=AdminDashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    await IdentitySeeder.SeedRolesAsync(services);
}

    app.Run();