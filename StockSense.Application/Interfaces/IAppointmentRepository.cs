using StockSense.Domain.Entities;

namespace StockSense.Application.Interfaces;

public interface IAppointmentRepository
{
    Task<List<Appointment>> GetAllAppointmentsAsync();
    Task<Appointment?> GetByIdAsync(int id);
    Task<Appointment> AddAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task DeleteAsync(int id);
}