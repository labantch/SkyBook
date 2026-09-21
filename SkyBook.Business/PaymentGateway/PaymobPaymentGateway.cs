using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using SkyBook.Business.PaymentGateway;

namespace SkyBook.Business.PaymentGateway
{
    public class PaymobPaymentGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PaymobPaymentGateway(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(
            PaymentRequest request)
        {
            var secretKey =
                _configuration["Paymob:ApiKey"];

            var integrationId =
                _configuration["Paymob:IntegrationId"];
            var hmacSecret = _configuration["Paymob:Hmac"];

            var baseUrl =
                _configuration["Paymob:BaseUrl"];

            var amountInCents =
                (int)(request.Amount * 100);

            var body = new
            {
                amount = amountInCents,
                currency = "EGP",

                payment_methods = new[]
                {
                    int.Parse(integrationId!)
                },
                notification_url=_configuration["Paymob:NotificationUrl"],
                redirection_url = _configuration["Paymob:RedirectionUrl"],

                items = new[]
                {
                    new
                    {
                        name = "SkyBook Flight Booking",
                        amount = amountInCents,
                        description = "Flight booking payment",
                        quantity = 1
                    }
                },

                billing_data = new
                {
                    first_name = string.IsNullOrWhiteSpace(request.CustomerName) ? "SkyBook" : request.CustomerName.Trim().Split(' ')[0],
                    last_name = string.IsNullOrWhiteSpace(request.CustomerName) || !request.CustomerName.Trim().Contains(' ') ? "Customer" : request.CustomerName.Trim().Substring(request.CustomerName.Trim().IndexOf(' ') + 1),
                    phone_number = string.IsNullOrWhiteSpace(request.CustomerPhone) ? "+201000000000" : request.CustomerPhone,
                    email = string.IsNullOrWhiteSpace(request.CustomerEmail) ? "customer@skybook.com" : request.CustomerEmail,

                    apartment = "NA",
                    street = "NA",
                    building = "NA",
                    city = "Cairo",
                    country = "EG",
                    floor = "NA",
                    state = "Cairo"
                }
            };

            var json = JsonSerializer.Serialize(body);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Token",
                    secretKey);

            var response = await _httpClient.PostAsync(
                $"{baseUrl}/v1/intention/",
                content);

            var responseContent =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = responseContent
                };
            }

            var result =
                JsonSerializer.Deserialize<JsonElement>(
                    responseContent);

            var clientSecret =
                result.GetProperty("client_secret").GetString();

            return new PaymentResult
            {
                IsSuccess = true,
                PaymentUrl =
                    $"{baseUrl}/unifiedcheckout/?publicKey={_configuration["Paymob:PublicKey"]}&clientSecret={clientSecret}",
                Message = "Payment intention created successfully."
            };
        }
    }
}