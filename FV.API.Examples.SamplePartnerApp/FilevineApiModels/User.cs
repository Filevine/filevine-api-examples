namespace FV.API.SamplePartnerApp.Models;

public class User
{
    public NativePartnerPair UserId { get; set; } = new();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}
