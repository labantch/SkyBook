using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Data.Models;

namespace SkyBook.Presentation.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }
        [HttpPost]
        public async Task<IActionResult> Pay(
      int bookingId,
      PaymentMethod paymentMethod)
        {
            var result = await _paymentService
                .ProcessPaymentAsync(
                    bookingId,
                    paymentMethod);

            if (result.IsSuccess)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }
            return RedirectToAction(  "MyBookings", "Booking");
        }
    }
}
