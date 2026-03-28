using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSense.Data;
using StockSense.shared;

namespace StockSense.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Protect entire controller
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // --- GET: Fetch all users and map to UserDto ---
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                userDtos.Add(new UserDto
                {
                    Id = u.Id,
                    Email = u.Email ?? "",
                    FullName = $"{u.FirstName} {u.LastName}",
                    Role = roles.FirstOrDefault() ?? "Customer",
                    // Blocked if LockoutEnd is in the future
                    IsBlocked = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow
                });
            }
            return Ok(userDtos);
        }

        // --- POST: Create a new Employee/Admin account ---
        [HttpPost("create-employee")]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, dto.Role);
                return Ok();
            }

            var firstError = result.Errors.FirstOrDefault()?.Description ?? "Registration failed";
            return BadRequest(firstError);
        }

        // --- POST: Update a User's Role ---
        [HttpPost("change-role")]
        public async Task<IActionResult> ChangeRole([FromBody] RoleChangeRequest req)
        {
            var user = await _userManager.FindByIdAsync(req.UserId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, req.NewRole);

            return Ok();
        }

        // --- POST: Toggle Account Lockout (Block/Unblock) ---
        [HttpPost("toggle-block/{id}")]
        public async Task<IActionResult> ToggleBlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
            {
                // Currently Blocked -> Unblock by clearing date
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            else
            {
                // Currently Active -> Block by setting date far in the future
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            }
            return Ok();
        }
    }

    public class RoleChangeRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string NewRole { get; set; } = string.Empty;
    }
}