using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;

namespace SkyBook.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly IApplicationUserService _userService;

        public UserManagementController(IApplicationUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }
    }
}
