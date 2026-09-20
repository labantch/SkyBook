using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Presentation.Models;

namespace SkyBook.Presentation.Controllers;

public class HomeController : Controller
{
    private readonly IAirportService _airportService;

    public HomeController(IAirportService airportService)
    {
        _airportService = airportService;
    }

    public async Task<IActionResult> Index()
    {
        var airports = await _airportService.GetAllAirportAsync();
        ViewBag.Airports = airports;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetAirports()
    {
        var airports = await _airportService.GetAllAirportAsync();
        return Json(airports);
    }

    public IActionResult About() => View();
    public IActionResult SkyClub() => View();
    public IActionResult Error() => View();
}

