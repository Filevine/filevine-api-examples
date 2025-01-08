namespace FV.API.SamplePartnerApp.Models;

public class Org
{
    public string Name { get; set; } = string.Empty;
    public int OrgId { get; set; }
    public ProjectTypes[]? ProjectTypes { get; set; }
}
