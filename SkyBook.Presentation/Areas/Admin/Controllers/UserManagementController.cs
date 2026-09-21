using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Data.Models;

namespace SkyBook.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class UserManagementController : Controller
    {
        private readonly IApplicationUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserManagementController(
            IApplicationUserService userService,
            UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();

            // Only SuperAdmins see the promote/demote controls
            ViewBag.IsSuperAdmin = User.IsInRole("SuperAdmin");

            return View(users);
        }

        // Only the SuperAdmin can promote a user to Admin
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Invalid user." });

            // Prevent promoting yourself (you're already SuperAdmin)
            var currentUserId = _userManager.GetUserId(User);
            if (userId == currentUserId)
                return Json(new { success = false, message = "You cannot change your own role." });

            var ok = await _userService.PromoteUserAsync(userId);
            return Json(new { success = ok, message = ok ? "User promoted to Admin." : "User not found." });
        }

        // Only the SuperAdmin can demote an Admin back to regular user
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DemoteUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Invalid user." });

            var currentUserId = _userManager.GetUserId(User);
            if (userId == currentUserId)
                return Json(new { success = false, message = "You cannot change your own role." });

            var ok = await _userService.DemoteUserAsync(userId);
            return Json(new { success = ok, message = ok ? "User demoted to regular user." : "User not found." });
        }
    }
}
