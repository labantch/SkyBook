using Microsoft.AspNetCore.Mvc;

namespace SkyBook.Presentation.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login() => View();
        public IActionResult Register() => View();
    }
}
