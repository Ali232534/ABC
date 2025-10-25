namespace HospitalManagementSystem.ViewModels;

public class DoctorViewModel
{
    public int DoctorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public TimeOnly AvailableFrom { get; set; }
    public TimeOnly AvailableTo { get; set; }
    public bool IsAvailable { get; set; }
    public string Address { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
    public int ExperienceYears { get; set; }
    public string Qualification { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
}

public class SpecializationCountViewModel
{
    public string Specialization { get; set; } = string.Empty;
    public int Count { get; set; }
}