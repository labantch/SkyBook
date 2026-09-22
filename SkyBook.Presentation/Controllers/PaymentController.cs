using Microsoft.AspNetCore.Mvc;
using SkyBook.Business.Interfaces;
using SkyBook.Data.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace SkyBook.Presentation.Controllers
{
    [Route("payment")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public PaymentController(IPaymentService paymentService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration;
        }

        [HttpGet("pay")]
        public async Task<IActionResult> Pay(int bookingId, PaymentMethod paymentMethod)
        {
            var result = await _paymentService.CreatePaymentAsync(bookingId, paymentMethod);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("MyBookings", "Booking");
            }

            return Redirect(result.PaymentUrl!);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] JsonElement data)
        {
            if (!data.TryGetProperty("obj", out var transaction))
            {
                return BadRequest("Invalid payload structure");
            }

            string receivedHmac = Request.Query["hmac"].ToString();
            if (string.IsNullOrEmpty(receivedHmac) && data.TryGetProperty("hmac", out var bodyHmac))
            {
                receivedHmac = bodyHmac.GetString() ?? string.Empty;
            }

            if (string.IsNullOrEmpty(receivedHmac) || !VerifyHmac(transaction, receivedHmac))
            {
                return Unauthorized();
            }

            var transactionId = transaction.GetProperty("id").GetInt32().ToString();
            var success = transaction.GetProperty("success").GetBoolean();
            var status = success ? PaymentStatus.Paid : PaymentStatus.Failed;

          
            await _paymentService.UpdatePaymentStatusAsync(transactionId, status);

            return Ok();
        }

        [HttpGet("success")]
        public async Task<IActionResult> Success([FromQuery] string? success, [FromQuery] string? id, [FromQuery] string? pending)
        {
          
            if (!string.IsNullOrEmpty(id) && success == "true")
            {
                await _paymentService.UpdatePaymentStatusAsync(id, PaymentStatus.Paid);
            }

            return View();
        }

        private bool VerifyHmac(JsonElement transaction, string receivedHmac)
        {
            var hmacSecret = _configuration["Paymob:Hmac"] ?? _configuration["Paymob:HmacSecret"];

            if (string.IsNullOrEmpty(hmacSecret))
                return false;

            string GetPropertyString(JsonElement element, string propertyName)
            {
                if (!element.TryGetProperty(propertyName, out var prop))
                    return string.Empty;

                return prop.ValueKind switch
                {
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    JsonValueKind.Null => "",
                    _ => prop.ToString()
                };
            }

            var values = new[]
            {
                GetPropertyString(transaction, "amount_cents"),
                GetPropertyString(transaction, "created_at"),
                GetPropertyString(transaction, "currency"),
                GetPropertyString(transaction, "error_occured"),
                GetPropertyString(transaction, "has_parent_transaction"),
                GetPropertyString(transaction, "id"),
                GetPropertyString(transaction, "integration_id"),
                GetPropertyString(transaction, "is_3d_secure"),
                GetPropertyString(transaction, "is_auth"),
                GetPropertyString(transaction, "is_capture"),
                GetPropertyString(transaction, "is_refunded"),
                GetPropertyString(transaction, "is_standalone_payment"),
                GetPropertyString(transaction, "is_void"),
                GetPropertyString(transaction, "is_voided"),
                transaction.TryGetProperty("order", out var order) ? GetPropertyString(order, "id") : "",
                GetPropertyString(transaction, "owner"),
                GetPropertyString(transaction, "pending"),
                transaction.TryGetProperty("source_data", out var source) ? GetPropertyString(source, "pan") : "",
                transaction.TryGetProperty("source_data", out source) ? GetPropertyString(source, "sub_type") : "",
                transaction.TryGetProperty("source_data", out source) ? GetPropertyString(source, "type") : "",
                GetPropertyString(transaction, "success")
            };

            var concatenatedValues = string.Concat(values);

            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hmacSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(concatenatedValues));
            var calculatedHmac = Convert.ToHexString(hash).ToLowerInvariant();

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(calculatedHmac),
                Encoding.UTF8.GetBytes(receivedHmac.ToLowerInvariant()));
        }
    }
}