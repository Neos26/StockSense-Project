namespace StockSense.Application.DTOs;

public class AppointmentDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TimeSlot { get; set; } = string.Empty;
    public string ServicesRequested { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string MechanicName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
}

public partial class CreateAppointmentDto
{
    public string CustomerName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; } = DateTime.Now;
    public string TimeSlot { get; set; } = string.Empty;
    public List<string> SelectedServices { get; set; } = new();
    public string Category { get; set; } = "General";
}

public class BookedSlotDto
{
    public string TimeSlot { get; set; } = string.Empty;
    public int EstimatedMinutes { get; set; }
}

public class MechanicAssignmentDto
{
    public string MechanicName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
}