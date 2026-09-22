using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

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

        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            var secretKey = (_configuration["Paymob:SecretKey"]
                         ?? _configuration["Paymob:ApiKey"])?.Trim();

            var publicKey = _configuration["Paymob:PublicKey"]?.Trim();
            var integrationIdStr = _configuration["Paymob:IntegrationId"]?.Trim();
            var baseUrl = _configuration["Paymob:BaseUrl"] ?? "https://accept.paymob.com";

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = "Paymob ApiKey/SecretKey is missing in appsettings.json."
                };
            }

            if (!int.TryParse(integrationIdStr, out var integrationId))
            {
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = "Invalid Paymob IntegrationId format."
                };
            }

            var amountInCents = (int)(request.Amount * 100);

            var customerName = request.CustomerName?.Trim();
            string firstName = "SkyBook";
            string lastName = "Customer";

            if (!string.IsNullOrWhiteSpace(customerName))
            {
                var nameParts = customerName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                firstName = nameParts[0];
                if (nameParts.Length > 1)
                {
                    lastName = string.Join(" ", nameParts.Skip(1));
                }
            }

            var body = new
            {
                amount = amountInCents,
                currency = "EGP",
                payment_methods = new[] { integrationId },
                notification_url = _configuration["Paymob:NotificationUrl"],
                redirection_url = _configuration["Paymob:RedirectionUrl"],
                items = new[]
                {
                    new
                    {
                        name = "SkyBook Flight Booking",
                        amount = amountInCents,
                        description = "Flight ticket booking payment",
                        quantity = 1
                    }
                },
                billing_data = new
                {
                    first_name = firstName,
                    last_name = lastName,
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
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var requestUrl = $"{baseUrl.TrimEnd('/')}/v1/intention/";
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = content
            };

            httpRequest.Headers.TryAddWithoutValidation("Authorization", $"Token {secretKey}");

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = $"Paymob API Error: {responseContent}"
                };
            }

            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (!result.TryGetProperty("client_secret", out var clientSecretProp) || string.IsNullOrEmpty(clientSecretProp.GetString()))
            {
                return new PaymentResult
                {
                    IsSuccess = false,
                    Message = "Failed to retrieve client_secret from Paymob response."
                };
            }

            var clientSecret = clientSecretProp.GetString();

        
            string extractedId = string.Empty;
            if (result.TryGetProperty("id", out var idProp))
            {
                extractedId = idProp.ToString();
            }
            else if (result.TryGetProperty("order", out var orderProp))
            {
                extractedId = orderProp.ToString();
            }

            var paymentUrl = $"https://accept.paymob.com/unifiedcheckout/?publicKey={publicKey}&clientSecret={clientSecret}";

            return new PaymentResult
            {
                IsSuccess = true,
                TransactionId = extractedId, 
                PaymentUrl = paymentUrl,
                Message = "Payment intention created successfully."
            };
        }
    }
}