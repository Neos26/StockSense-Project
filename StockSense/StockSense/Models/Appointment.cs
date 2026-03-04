
public class Appointment
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; } = DateTime.Now;
    public string ServiceType { get; set; } = "General Checkup";
}
