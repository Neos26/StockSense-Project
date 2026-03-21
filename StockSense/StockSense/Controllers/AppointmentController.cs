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

    public AppointmentsController(ApplicationDbContext db) => _db = db;

    // --- CREATE: Save Appointment with Category ---
    [HttpPost]
    public async Task<IActionResult> Create(Appointment appt)
    {
        // Ensure new appointments always start as Pending
        appt.Status = "Pending";

        // If the category is missing for some reason, default it
        if (string.IsNullOrWhiteSpace(appt.Category))
        {
            appt.Category = "General";
        }

        _db.Appointments.Add(appt);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Appointment booked successfully!", id = appt.Id });
    }

    // --- GET: Booked Slots for Date Picker ---
    [HttpGet("booked-slots")]
    public async Task<ActionResult<List<string>>> GetBookedSlots([FromQuery] DateTime date)
    {
        var bookedSlots = await _db.Appointments
            .Where(a => a.AppointmentDate.Date == date.Date && a.Status != "Cancelled")
            .Select(a => a.TimeSlot)
            .ToListAsync();

        return Ok(bookedSlots);
    }

    // --- GET: All Appointments for Admin Table ---
    [HttpGet("all")]
    public async Task<ActionResult<List<Appointment>>> GetAllAppointments()
    {
        // Now includes the Category field automatically
        var appointments = await _db.Appointments
            .OrderByDescending(a => a.AppointmentDate)
            .ThenBy(a => a.TimeSlot)
            .ToListAsync();

        return Ok(appointments);
    }

    // --- UPDATE: Status (Confirmed, Completed, etc.) ---
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
    {
        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        appointment.Status = newStatus;
        await _db.SaveChangesAsync();

        return Ok();
    }

    // --- GET: Specific User's Appointments ---
    [HttpGet("my-bookings")]
    public async Task<ActionResult<List<Appointment>>> GetMyBookings([FromQuery] string name)
    {
        var myBookings = await _db.Appointments
            .Where(a => a.CustomerName == name)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        return Ok(myBookings);
    }
}