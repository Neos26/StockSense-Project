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

        // --- GET: Fetch all users mapped to UserDto ---
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            // Using Projection (Select) is much faster than a loop
            var users = await _userManager.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email ?? "",
                    FullName = $"{u.FirstName} {u.LastName}",
                    Role = u.Role, // Uses the custom Role column from migration
                    IsBlocked = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow
                })
                .ToListAsync();

            return Ok(users);
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
                LastName = dto.LastName,
                Role = dto.Role // Save role to the custom column
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

            // Update Identity System
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, req.NewRole);

            // Update custom column for fast fetching
            user.Role = req.NewRole;
            await _userManager.UpdateAsync(user);

            return Ok();
        }

        // --- POST: Toggle Account Lockout ---
        [HttpPost("toggle-block/{id}")]
        public async Task<IActionResult> ToggleBlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            }
            return Ok();
        }

        // --- DELETE: Remove User Permanently ---
        // Triggered by ExecuteDelete in SystemManagement.razor
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Safety check: Don't let an admin delete themselves
            var currentUserId = _userManager.GetUserId(User);
            if (id == currentUserId)
            {
                return BadRequest("You cannot delete your own admin account.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Ok(new { message = "User deleted successfully" });
            }

            return BadRequest("Failed to delete user.");
        }
    }

    public class RoleChangeRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string NewRole { get; set; } = string.Empty;
    }
}
