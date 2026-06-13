using StockSense.Application.DTOs;

namespace StockSense.Application.Interfaces;

public interface IAppointmentService
{
    Task<List<AppointmentDto>> GetAllAppointmentsAsync();
    Task<AppointmentDto> BookAppointmentAsync(CreateAppointmentDto request);
    Task<bool> UpdateStatusAsync(int appointmentId, string newStatus);
    Task<bool> FinalizeAppointmentAsync(int appointmentId, string mechanicName, decimal finalCost);

    // New methods extracted from the controller
    Task<List<BookedSlotDto>> GetBookedSlotsAsync(DateTime date, string? mechanic);
    Task<bool> AssignMechanicAsync(int id, MechanicAssignmentDto assignment);
    Task<List<AppointmentDto>> GetMyBookingsAsync(string customerName);
}