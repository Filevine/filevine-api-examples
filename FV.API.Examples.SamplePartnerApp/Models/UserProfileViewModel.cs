using System.Security.Claims;

namespace FV.API.SamplePartnerApp.Models;

public class UserProfileViewModel
{
    public OrgUser OrgUser { get; set; }
    public UserOrgs UserOrgs { get; set; }
    public IDictionary<string, string?> TokenProperties { get; set; }
    public IEnumerable<Claim> Claims { get; set; }
}