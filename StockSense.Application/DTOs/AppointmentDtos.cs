namespace StockSense.Application.DTOs;

// What the UI reads to display the appointment list
public class AppointmentDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string TimeSlot { get; set; } = string.Empty;
    public string ServicesRequested { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string MechanicName { get; set; } = string.Empty;
}
// What the customer sends when booking an appointment
public partial class CreateAppointmentDto
{
    public string CustomerName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; } = DateTime.Now;
    public string TimeSlot { get; set; } = string.Empty; 
    
    // The UI can send a list of selected services (e.g., ["Change Oil", "Tune Up"])
    // Your service will flatten this into the string your database expects.
    public List<string> SelectedServices { get; set; } = new(); 
    
    public string Category { get; set; } = "General";
}