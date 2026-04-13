
using System.ComponentModel.DataAnnotations.Schema;

public class Appointment
{
    public int Id { get; set; }

    // Customer Info (You can add Phone Number, Email, etc. here later)
    public string CustomerName { get; set; } = string.Empty;

    // Date and Time
    public DateTime AppointmentDate { get; set; } = DateTime.Now;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string TimeSlot { get; set; } = string.Empty; // Stores "09:00", "10:15", etc.

    // Service Details
    public string ServicesRequested { get; set; } = string.Empty; // Stores "Change Oil, Air Filter Cleaning"

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; } // Stores the calculated P900

    public string Status { get; set; } = "Pending";

    public string Category { get; set; } = "General";
    public string MechanicName { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }


}
