using StockSense.Application.DTOs;

namespace StockSense.Application.Interfaces;

public interface IAppointmentService
{
    Task<List<AppointmentDto>> GetAllAppointmentsAsync();
    
    // Handles the booking logic and flattens the SelectedServices list
    Task<AppointmentDto> BookAppointmentAsync(CreateAppointmentDto request);
    
    // Updates status (e.g., from "Pending" to "Confirmed" or "Completed")
    Task<bool> UpdateStatusAsync(int appointmentId, string newStatus);
    
    // Assigns a mechanic and potentially updates the final cost
    Task<bool> FinalizeAppointmentAsync(int appointmentId, string mechanicName, decimal finalCost);
}