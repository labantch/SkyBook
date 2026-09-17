namespace SkyBook.Business.ViewModels;

public class UserVM
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Country { get; set; }
    public string? ImageUrl { get; set; }
    public string Role { get; set; } = "User";
}
