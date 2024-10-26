using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
public class HomeController : ControllerBase
{
    [HttpGet("")]
    [ApiExplorerSettings(IgnoreApi = true)]
     public IActionResult RedirectToSwagger()
    {
        return Redirect("https://localhost:44328/swagger/index.html");
    }
}