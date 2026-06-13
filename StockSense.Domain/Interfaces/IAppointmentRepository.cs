using StockSense.Domain.Entities;

namespace StockSense.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<List<Appointment>> GetAllAppointmentsAsync();
    Task<Appointment?> GetByIdAsync(int id);
    Task<Appointment> AddAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task DeleteAsync(int id);
    Task<List<Appointment>> GetAppointmentsByDateAndMechanicAsync(DateTime date, string? mechanic);
    Task<List<Appointment>> GetByCustomerNameAsync(string customerName);
}
