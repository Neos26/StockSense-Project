using StockSense.Application.DTOs;
using StockSense.Application.Interfaces;
using StockSense.Domain.Entities;
using StockSense.Domain.Interfaces;

namespace StockSense.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _repository;

    private static readonly TimeZoneInfo PhZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");

    public AppointmentService(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AppointmentDto>> GetAllAppointmentsAsync()
    {
        var appointments = await _repository.GetAllAppointmentsAsync();

        return appointments.Select(MapToDto).ToList();
    }

    public async Task<AppointmentDto> BookAppointmentAsync(CreateAppointmentDto request)
    {
        string flatServices = string.Join(", ", request.SelectedServices);

        DateTime phNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PhZone);

        var newAppointment = new Appointment
        {
            CustomerName = request.CustomerName,
            AppointmentDate = DateTime.SpecifyKind(request.AppointmentDate.Date, DateTimeKind.Unspecified),
            TimeSlot = request.TimeSlot,
            Category = string.IsNullOrWhiteSpace(request.Category) ? "General Service" : request.Category,
            ServicesRequested = flatServices,
            Status = "Pending",
            CreatedAt = phNow,
            TotalAmount = 0,
            MechanicName = "Unassigned"
        };

        var savedAppointment = await _repository.AddAsync(newAppointment);

        return MapToDto(savedAppointment);
    }

    public async Task<bool> UpdateStatusAsync(int appointmentId, string newStatus)
    {
        var appointment = await _repository.GetByIdAsync(appointmentId);
        if (appointment == null) return false;

        appointment.Status = newStatus;
        await _repository.UpdateAsync(appointment);
        return true;
    }

    public async Task<bool> FinalizeAppointmentAsync(int appointmentId, string mechanicName, decimal finalCost)
    {
        var appointment = await _repository.GetByIdAsync(appointmentId);
        if (appointment == null) return false;

        appointment.MechanicName = mechanicName;
        appointment.TotalAmount = finalCost;
        appointment.Status = "Completed";

        await _repository.UpdateAsync(appointment);
        return true;
    }

    public async Task<List<BookedSlotDto>> GetBookedSlotsAsync(DateTime date, string? mechanic)
    {
        var appointments = await _repository.GetAppointmentsByDateAndMechanicAsync(date, mechanic);

        return appointments.Select(a => new BookedSlotDto
        {
            TimeSlot = a.TimeSlot,
            EstimatedMinutes = a.DurationMinutes
        }).ToList();
    }

    public async Task<bool> AssignMechanicAsync(int id, MechanicAssignmentDto assignment)
    {
        var appointment = await _repository.GetByIdAsync(id);
        if (appointment == null) return false;

        appointment.MechanicName = assignment.MechanicName;
        appointment.DurationMinutes = assignment.DurationMinutes;
        appointment.Status = "Confirmed";

        await _repository.UpdateAsync(appointment);
        return true;
    }

    public async Task<List<AppointmentDto>> GetMyBookingsAsync(string customerName)
    {
        var appointments = await _repository.GetByCustomerNameAsync(customerName);
        return appointments.Select(MapToDto).ToList();
    }

    private static AppointmentDto MapToDto(Appointment a)
    {
        return new AppointmentDto
        {
            Id = a.Id,
            CustomerName = a.CustomerName,
            AppointmentDate = a.AppointmentDate,
            CreatedAt = a.CreatedAt,
            TimeSlot = a.TimeSlot,
            ServicesRequested = a.ServicesRequested,
            TotalAmount = a.TotalAmount,
            Status = a.Status,
            Category = a.Category,
            MechanicName = a.MechanicName,
            DurationMinutes = a.DurationMinutes
        };
    }
}