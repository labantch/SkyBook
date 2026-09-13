using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SkyBook.Presentation.Models;

namespace SkyBook.Presentation.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
    public IActionResult About() => View();
    public IActionResult SkyClub() => View();
    public IActionResult Error() => View();
}
