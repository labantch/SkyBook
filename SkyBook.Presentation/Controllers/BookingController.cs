using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Business.ViewModels;
using SkyBook.Data.Models;

namespace SkyBook.Presentation.Controllers
{
   
           [Authorize]
        public class BookingController : Controller
        {
            private readonly IBookingService _bookingService;
            private readonly UserManager<ApplicationUser> _userManager;

            public BookingController(
                IBookingService bookingService,
                UserManager<ApplicationUser> userManager)
            {
                _bookingService = bookingService;
                _userManager = userManager;
            }

        #region MyBookings
        public async Task<IActionResult> MyBookings()
            {
                var userId = _userManager.GetUserId(User);
                var bookings = await _bookingService.GetUserBookingsAsync(userId);
                return View(bookings);
            }
        #endregion

        #region Details
        public async Task<IActionResult> Details(int id)
            {
                var userId = _userManager.GetUserId(User);
                var booking =await _bookingService.GetBookingByIdAsync(id, userId);
                return View(booking);
            }
        #endregion

        #region CreateGet
        [HttpGet]
            public IActionResult Create( int flightId,int seatId)
            {
                var model = new CreateBookingVM
                {
                    FlightId = flightId,
                    SeatId = seatId
                };
                return View(model);
            }
        #endregion

        #region CreatePost
        [HttpPost]
            [ValidateAntiForgeryToken]
    
        public async Task<IActionResult> Create(  CreateBookingVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User);

            try
            {
                var bookingId =
                    await _bookingService.CreateBookingAsync( userId, model);
                return RedirectToAction( "Pay","Payment",
                    new
                    {
                        bookingId = bookingId,
                        paymentMethod = PaymentMethod.Card
                    });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(  "",ex.Message);
                return View(model);
            }
        }
        #endregion

        #region Cancel
        [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Cancel(
                int id)
            {
                var userId = _userManager.GetUserId(User);
                try
                {
                    await _bookingService.CancelAsync(id,userId);

                    TempData["Success"] ="Booking cancelled successfully.";
                }
                catch (Exception ex)
                {
                    TempData["Error"] =ex.Message;
                }
                return RedirectToAction(nameof(MyBookings));
            }
        #endregion
    }
}
