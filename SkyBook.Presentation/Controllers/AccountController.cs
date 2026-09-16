using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Models;

namespace SkyBook.Presentation.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApplicationUserService _userService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(IApplicationUserService userService, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager)
        {
            _userService = userService;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }
        public IActionResult login()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task< IActionResult> Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                await _userService.LoginAsync(model);
                return RedirectToAction(nameof(Index)); 
            }
            return View();
        } 
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register() => View();

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
            await _roleManager.CreateAsync(new IdentityRole("Passenger"));
            if (ModelState.IsValid)
            {
                await _userService.RegisterAsync(model);
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("login");
        }
    }
}
