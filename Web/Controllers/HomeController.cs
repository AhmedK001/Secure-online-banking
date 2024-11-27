using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
public class HomeController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _contextAccessor;

    public HomeController(IConfiguration configuration, IHttpContextAccessor contextAccessor)
    {
        _configuration = configuration;
        _contextAccessor = contextAccessor;
    }
    [HttpGet("")]
    [ApiExplorerSettings(IgnoreApi = true)]
     public IActionResult RedirectToSwagger()
     {
         var protocol = _contextAccessor.HttpContext.Request.Scheme;

         var host = _contextAccessor.HttpContext.Request.Host.ToString();

         var baseUrl = $"{protocol}://{host}";

         var redirectUrl = $"{baseUrl}/swagger";
        return Redirect(redirectUrl);
    }
}