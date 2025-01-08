using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FV.API.SamplePartnerApp.Models;
using FV.API.SamplePartnerApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FV.API.SamplePartnerApp.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly FvApiGatewayService _fvApiGatewayService;

    public UserController(FvApiGatewayService fvApiGatewayService)
    {
        _fvApiGatewayService = fvApiGatewayService;
    }
    
    public async Task<IActionResult> ProfileAsync()
    {
        var authResult = await HttpContext.AuthenticateAsync();
        var tokenProperties = authResult.Properties?.Items ?? new Dictionary<string, string?>();
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = (JwtSecurityToken) handler.ReadToken(await HttpContext.GetTokenAsync("id_token"));

        // get list of all orgs for user
        var userOrgs = await _fvApiGatewayService.GetUserOrgsForToken(accessToken??"");

        // get current native user id and first org id.
        // There might be multiple orgs, in which case we want to ask the user which org they want to use.

        // get user info
        var orgUser = await _fvApiGatewayService.GetCurrentUser(accessToken??"", userOrgs.User.UserId.Native, userOrgs.Orgs.First().OrgId);

        ViewBag.OrgUser = orgUser;
        ViewBag.Orgs = userOrgs;
        ViewBag.Claims = jsonToken.Claims.OrderBy(c => c.Type).ToList();
        ViewBag.TokenProperties = new SortedDictionary<string, string?>(tokenProperties);
        return View();
    }
}