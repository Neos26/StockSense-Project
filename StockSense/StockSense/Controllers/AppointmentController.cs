using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSense.Data;
using Microsoft.AspNetCore.Authorization;

namespace StockSense.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    // Standard Windows/Azure ID for Philippine Time (UTC+8)
    private static readonly TimeZoneInfo PhZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");

    public AppointmentsController(ApplicationDbContext db) => _db = db;

    // --- 1. CREATE: Save Appointment ---
    [HttpPost]
    public async Task<IActionResult> Create(Appointment appt)
    {
        // FIX 1: Explicitly calculate PH time for the creation log
        DateTime phNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PhZone);
        appt.CreatedAt = phNow;

        // FIX 2: "The Nuclear Fix" for the Date Shift
        // Strips any timezone offset coming from Blazor and treats it as a 'Dumb' local date.
        // This prevents the server from subtracting 8 hours and moving the date to the previous day.
        appt.AppointmentDate = DateTime.SpecifyKind(appt.AppointmentDate.Date, DateTimeKind.Unspecified);

        appt.Status = "Pending";

        if (string.IsNullOrWhiteSpace(appt.Category))
        {
            appt.Category = "General Service";
        }

        if (string.IsNullOrWhiteSpace(appt.MechanicName))
        {
            appt.MechanicName = "Unassigned";
        }

        try
        {
            _db.Appointments.Add(appt);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Appointment booked successfully!", id = appt.Id });
        }
        catch (Exception ex)
        {
            // Log error for debugging if database save fails
            return StatusCode(500, new { message = "Database Error", error = ex.Message });
        }
    }

    // --- 2. GET: Booked Slots ---
    [HttpGet("booked-slots")]
    public async Task<IActionResult> GetBookedSlots([FromQuery] DateTime date, [FromQuery] string mechanic)
    {
        var searchDate = date.Date;

        var query = _db.Appointments
            .Where(a => a.AppointmentDate.Date == searchDate && a.Status != "Cancelled");

        if (!string.IsNullOrEmpty(mechanic) && mechanic != "Any Available")
        {
            query = query.Where(a => a.MechanicName == mechanic);
        }

        // UPDATED: Now grabbing TimeSlot and EstimatedMinutes
        var bookedSlots = await query
            .Select(a => new { a.TimeSlot, EstimatedMinutes = a.DurationMinutes })
            .ToListAsync();

        return Ok(bookedSlots);
    }

    // --- 3. GET: All Appointments (Admin Dashboard) ---
    [HttpGet("all")]
    public async Task<ActionResult<List<Appointment>>> GetAllAppointments()
    {
        return await _db.Appointments
            .OrderByDescending(a => a.AppointmentDate)
            .ThenBy(a => a.TimeSlot)
            .ToListAsync();
    }

    // --- 4. UPDATE: Assign Mechanic ---
    [HttpPut("{id}/assign-mechanic")]
    public async Task<IActionResult> AssignMechanic(int id, [FromBody] MechanicAssignmentDto assignment)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        appointment.MechanicName = assignment.MechanicName;
        appointment.DurationMinutes = assignment.DurationMinutes;
        appointment.Status = "Confirmed";

        await _db.SaveChangesAsync();
        return Ok(new { message = $"Assigned to {assignment.MechanicName}" });
    }

    // --- 5. UPDATE: Status ---
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        appointment.Status = newStatus;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Status updated" });
    }

    // --- 6. GET: My Bookings ---
    [HttpGet("my-bookings")]
    public async Task<ActionResult<List<Appointment>>> GetMyBookings([FromQuery] string name)
    {
        // Sorting by the new PH-based CreatedAt timestamp
        return await _db.Appointments
            .Where(a => a.CustomerName == name)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}

public class MechanicAssignmentDto
{
    public string MechanicName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
}