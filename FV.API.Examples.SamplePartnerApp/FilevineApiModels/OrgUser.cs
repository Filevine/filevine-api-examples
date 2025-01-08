namespace FV.API.SamplePartnerApp.Models;

public class OrgUser
{
    public NativePartnerPair OrgUserId { get; set; } = new();
    public string UserName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = false;
    public DateTime CreatedDateTime { get; set; } = DateTime.MinValue;
    public string Email { get; set; } = string.Empty;
    public Org Org { get; set; } = new();
    public User User { get; set; } = new();
}
