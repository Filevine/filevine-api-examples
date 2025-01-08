namespace FV.API.SamplePartnerApp.Services;

public static class FvV2ApiPaths
{
    public const string ApiPath = "fv-app";
    public const string GetMe = $"{ApiPath}/v2/Users/Me";
    public const string GetUserOrgsForToken = $"{ApiPath}/v2/utils/GetUserOrgsWithToken";
}