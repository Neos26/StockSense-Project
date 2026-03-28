using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSense.Data;
using Microsoft.AspNetCore.Authorization;
using StockSense.shared; // Ensure your Appointment and Mechanic models are here

namespace StockSense.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AppointmentsController(ApplicationDbContext db) => _db = db;

    // --- 1. CREATE: Save Appointment ---
    [HttpPost]
    public async Task<IActionResult> Create(Appointment appt)
    {
        // Set Default Values for new Azure Columns
        appt.Status = "Pending";
        appt.CreatedAt = DateTime.UtcNow;

        // Ensure Category isn't null for the DB
        if (string.IsNullOrWhiteSpace(appt.Category))
        {
            appt.Category = "General Service";
        }

        // Default Mechanic to Unassigned until Admin picks one
        if (string.IsNullOrWhiteSpace(appt.MechanicName))
        {
            appt.MechanicName = "Unassigned";
        }

        _db.Appointments.Add(appt);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Appointment booked successfully!", id = appt.Id });
    }

    // --- 2. GET: Booked Slots (Used by the MudBlazor/HTML DatePicker) ---
    [HttpGet("booked-slots")]
    public async Task<ActionResult<List<string>>> GetBookedSlots([FromQuery] DateTime date)
    {
        var bookedSlots = await _db.Appointments
            .Where(a => a.AppointmentDate.Date == date.Date && a.Status != "Cancelled")
            .Select(a => a.TimeSlot)
            .ToListAsync();

        return Ok(bookedSlots);
    }

    // --- 3. GET: All Appointments (Admin Dashboard) ---
    [HttpGet("all")]
    public async Task<ActionResult<List<Appointment>>> GetAllAppointments()
    {
        // Returns the full model including the new MechanicName and DurationMinutes
        return await _db.Appointments
            .OrderByDescending(a => a.AppointmentDate)
            .ThenBy(a => a.TimeSlot)
            .ToListAsync();
    }

    // --- 4. UPDATE: Assign Mechanic and Duration ---
    // This uses the new columns we added to the Appointments table
    [HttpPut("{id}/assign-mechanic")]
    public async Task<IActionResult> AssignMechanic(int id, [FromBody] MechanicAssignmentDto assignment)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        appointment.MechanicName = assignment.MechanicName;
        appointment.DurationMinutes = assignment.DurationMinutes;
        appointment.Status = "Confirmed"; // Move from Pending to Confirmed

        await _db.SaveChangesAsync();
        return Ok(new { message = $"Assigned to {assignment.MechanicName}" });
    }

    // --- 5. UPDATE: General Status (Completed, Cancelled, etc.) ---
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        appointment.Status = newStatus;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Status updated" });
    }

    // --- 6. GET: Specific Customer Bookings ---
    [HttpGet("my-bookings")]
    public async Task<ActionResult<List<Appointment>>> GetMyBookings([FromQuery] string name)
    {
        return await _db.Appointments
            .Where(a => a.CustomerName == name)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}

// Helper DTO for the assignment endpoint
// You can put this in your StockSense.shared namespace
public class MechanicAssignmentDto
{
    public string MechanicName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
}