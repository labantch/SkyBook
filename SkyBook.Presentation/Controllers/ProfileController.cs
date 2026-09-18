using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using System.Security.Claims;

namespace SkyBook.Presentation.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly IBookingService _bookingService;

        public ProfileController(
            IProfileService profileService,
            IBookingService bookingService)
        {
            _profileService = profileService;
            _bookingService = bookingService;
        }

        #region Details

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var model = await _profileService.GetProfileAsync(userId);

            if (model == null)
                return NotFound();

            return View(model);
        }

        #endregion

        #region Edit Profile

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var model = await _profileService.GetProfileAsync(userId);

            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var result =
                await _profileService.UpdateProfileAsync(userId, model);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] =
                    "Profile updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        #endregion

        #region Change Password

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var result =
                await _profileService.ChangePasswordAsync(
                    userId,
                    model);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] =
                    "Password changed successfully.";

                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        #endregion

        #region My Bookings

        public async Task<IActionResult> MyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var bookings =
                await _bookingService.GetUserBookingsAsync(userId!);

            return View(bookings);
        }

        #endregion

        #region Booking Details
    
      public async Task<IActionResult> BookingDetails(int id)
           {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var booking =
                await _bookingService.GetBookingByIdAsync(id, userId!);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        #endregion

        #region Cancel Booking

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _bookingService.CancelAsync(id, userId!);

            TempData["SuccessMessage"] =
                "Booking cancelled successfully.";

            return RedirectToAction(nameof(MyBookings));
        }

        #endregion
    }
}
