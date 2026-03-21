using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StockSense.Data;
using Microsoft.AspNetCore.Authorization;

namespace StockSense.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]

    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var profile = new UserProfile
            {
                FirstName = user.FirstName ?? "Valued",
                LastName = user.LastName ?? "Customer"
            };

            return Ok(profile);
        }
    }
}