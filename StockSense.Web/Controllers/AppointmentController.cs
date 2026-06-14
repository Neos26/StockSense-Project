using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StockSense.Application.DTOs;
using StockSense.Application.Interfaces;

namespace StockSense.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
    {
        try
        {
            var result = await _appointmentService.BookAppointmentAsync(dto);
            return Ok(new { message = "Appointment booked successfully!", id = result.Id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Error(ex.Message));
        }
    }

    [HttpGet("booked-slots")]
    public async Task<IActionResult> GetBookedSlots([FromQuery] DateTime date, [FromQuery] string? mechanic)
    {
        var bookedSlots = await _appointmentService.GetBookedSlotsAsync(date, mechanic);
        return Ok(bookedSlots);
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<AppointmentDto>>> GetAllAppointments()
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync();
        return Ok(appointments);
    }

    [HttpPut("{id}/assign-mechanic")]
    public async Task<IActionResult> AssignMechanic(int id, [FromBody] MechanicAssignmentDto assignment)
    {
        var success = await _appointmentService.AssignMechanicAsync(id, assignment);
        if (!success) return NotFound(ApiResponse.NotFound("Appointment"));
        return Ok(new { message = $"Assigned to {assignment.MechanicName}" });
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
    {
        var success = await _appointmentService.UpdateStatusAsync(id, newStatus);
        if (!success) return NotFound(ApiResponse.NotFound("Appointment"));
        return Ok(new { message = "Status updated" });
    }

    [HttpGet("my-bookings")]
    public async Task<ActionResult<List<AppointmentDto>>> GetMyBookings([FromQuery] string name)
    {
        var bookings = await _appointmentService.GetMyBookingsAsync(name);
        return Ok(bookings);
    }
}