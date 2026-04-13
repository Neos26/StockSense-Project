using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    [Authorize]
    [HttpGet("status")]
    public IActionResult GetStatus() => Ok(); // Returns 200 if cookie is valid
}
