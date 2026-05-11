using StockSense.Application.DTOs;
using StockSense.Application.Interfaces;
using StockSense.Domain.Entities;

namespace StockSense.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _repository;

    public AppointmentService(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AppointmentDto>> GetAllAppointmentsAsync()
    {
        var appointments = await _repository.GetAllAppointmentsAsync();
        
        // Map the database entities to the UI-friendly DTOs
        return appointments.Select(a => new AppointmentDto
        {
            Id = a.Id,
            CustomerName = a.CustomerName,
            AppointmentDate = a.AppointmentDate,
            TimeSlot = a.TimeSlot,
            ServicesRequested = a.ServicesRequested,
            TotalAmount = a.TotalAmount,
            Status = a.Status,
            MechanicName = a.MechanicName
        }).ToList();
    }

    public async Task<AppointmentDto> BookAppointmentAsync(CreateAppointmentDto request)
    {
        // 1. Flatten the list of services into a comma-separated string
        string flatServices = string.Join(", ", request.SelectedServices);

        // 2. Create the raw database entity
        var newAppointment = new Appointment
        {
            CustomerName = request.CustomerName,
            AppointmentDate = request.AppointmentDate,
            TimeSlot = request.TimeSlot,
            Category = request.Category,
            ServicesRequested = flatServices,
            Status = "Pending",
            CreatedAt = DateTime.Now,
            TotalAmount = 0 // Cost is calculated later by the mechanic in this flat setup
        };

        // 3. Save to database
        var savedAppointment = await _repository.AddAsync(newAppointment);

        // 4. Return the new DTO to the UI
        return new AppointmentDto
        {
            Id = savedAppointment.Id,
            CustomerName = savedAppointment.CustomerName,
            AppointmentDate = savedAppointment.AppointmentDate,
            TimeSlot = savedAppointment.TimeSlot,
            ServicesRequested = savedAppointment.ServicesRequested,
            TotalAmount = savedAppointment.TotalAmount,
            Status = savedAppointment.Status
        };
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
}