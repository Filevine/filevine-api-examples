using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FV.API.SamplePartnerApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace FV.API.SamplePartnerApp.Controllers;

[Authorize]
public class HomeController : Controller
{
    public HomeController()
    {

    }

    [AllowAnonymous]
    public IActionResult Index()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}