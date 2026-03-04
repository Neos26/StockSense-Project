// Controllers/AppointmentsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSense.Data;
namespace StockSense.Shared;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly ApplicationDbContext _db; // Assumed your DB context name

    public AppointmentsController(ApplicationDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Create(Appointment appt)
    {
        _db.Appointments.Add(appt);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Appointment booked successfully!" });
    }




    [HttpGet("booked-slots")]
    public async Task<ActionResult<List<string>>> GetBookedSlots([FromQuery] DateTime date)
    {
        // Search the database for appointments matching the requested date
        // and return ONLY the time slots (e.g., "09:00", "14:30")
        var bookedSlots = await _db.Appointments
            .Where(a => a.AppointmentDate.Date == date.Date)
            .Select(a => a.TimeSlot)
            .ToListAsync();

        return Ok(bookedSlots);
    }
}


