namespace HospitalManagementSystem.ViewModels;

public class AppointmentViewModel
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly AppointmentTime { get; set; }
    public string Status { get; set; } = "Scheduled";
    public string Description { get; set; } = string.Empty;
    public string Symptoms { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Prescription { get; set; } = string.Empty;

    // Additional properties for display
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorSpecialization { get; set; } = string.Empty;
    public string PatientPhone { get; set; } = string.Empty;
    public string DoctorPhone { get; set; } = string.Empty;

    public bool IsUpcoming => AppointmentDate > DateOnly.FromDateTime(DateTime.Today) ||
                             (AppointmentDate == DateOnly.FromDateTime(DateTime.Today) &&
                              AppointmentTime > TimeOnly.FromDateTime(DateTime.Now));

    public bool IsToday => AppointmentDate == DateOnly.FromDateTime(DateTime.Today);
}

public class CreateAppointmentViewModel
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly AppointmentTime { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Symptoms { get; set; } = string.Empty;

    public List<DoctorViewModel>? AvailableDoctors { get; set; }
    public List<PatientViewModel>? Patients { get; set; }
}

public class DashboardViewModel
{
    public int TotalDoctors { get; set; }
    public int TotalPatients { get; set; }
    public int TotalAppointments { get; set; }
    public int TodaysAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int PendingBills { get; set; }
}