namespace HospitalManagementSystem.ViewModels;

public class ReportViewModel
{
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public string ReportType { get; set; } = "Appointments"; // Appointments, Revenue, Patients, Doctors
}

public class AppointmentReportViewModel
{
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public List<AppointmentViewModel> Appointments { get; set; } = new();
}

public class FinancialReportViewModel
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalPendingAmount { get; set; }
    public int TotalBills { get; set; }
    public int PaidBills { get; set; }
    public int PendingBills { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public List<BillingViewModel> Bills { get; set; } = new();
    public List<RevenueReportViewModel> RevenueData { get; set; } = new();
}
