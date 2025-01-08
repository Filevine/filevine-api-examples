using System.Net.Http.Headers;
using FV.API.SamplePartnerApp.Models;
using Newtonsoft.Json;

namespace FV.API.SamplePartnerApp.Services;

public class FvApiGatewayService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FvApiGatewayService> _logger;
    private readonly HttpClient _gatewayClient;

    public FvApiGatewayService(
        IConfiguration configuration,
        ILogger<FvApiGatewayService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _gatewayClient = new HttpClient
        {
            BaseAddress = new Uri(_configuration["FvApiGateway:Url"]),
        };
    }

    public async Task<OrgUser?> GetCurrentUser(string accessToken, string userId, int orgId)
    {
        var message = new HttpRequestMessage(HttpMethod.Get, FvV2ApiPaths.GetMe);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        message.Headers.Add("x-fv-orgid", orgId.ToString());
        message.Headers.Add("x-fv-userid", userId);

        var response = await _gatewayClient.SendAsync(message); 

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var parsedResponse = JsonConvert.DeserializeObject<OrgUser>(content);
            return parsedResponse;
        }
        else
        {
            _logger.LogError("Error getting current user: {ResponseStatusCode}", response.StatusCode);
            throw new Exception($"Error getting current user: {response.StatusCode}");
        }
    }

    public async Task<UserOrgs> GetUserOrgsForToken(string accessToken)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, FvV2ApiPaths.GetUserOrgsForToken);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _gatewayClient.SendAsync(message);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var parsedResponse = JsonConvert.DeserializeObject<UserOrgs>(content);
            return parsedResponse;
        }
        else
        {
            _logger.LogError("Error getting user orgs for token: {ResponseStatusCode}", response.StatusCode);
            throw new Exception($"Error getting user orgs for token: {response.StatusCode}");
        }
    }
}