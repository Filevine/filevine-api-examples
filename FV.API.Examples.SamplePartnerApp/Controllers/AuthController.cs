using System.Security.Claims;
using FV.API.SamplePartnerApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace FV.API.SamplePartnerApp.Controllers;

public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(
        ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> Login()
    {        
        return RedirectToAction("Profile", "User");
    }

    public async Task<IActionResult> Logout()
    {
        return base.SignOut(OpenIdConnectDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
    }
}