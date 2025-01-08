using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FV.API.SamplePartnerApp.Controllers
{
    [Route("health-check")]
    [ApiController]
    public class HealthCheckController : Controller
    {
        [AllowAnonymous]
        [HttpGet("run")]
        public IActionResult Run()
        {
            return Ok("Healthy");
        }
    }
}
