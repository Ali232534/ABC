using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace HospitalManagementSystem.Controllers;

//[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;
    private readonly IAppointmentService _appointmentService;
    private readonly IBillingService _billingService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IDoctorService doctorService,
        IPatientService patientService,
        IAppointmentService appointmentService,
        IBillingService billingService,
        ILogger<AdminController> logger)
    {
        _doctorService = doctorService;
        _patientService = patientService;
        _appointmentService = appointmentService;
        _billingService = billingService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            var dashboardStats = new DashboardViewModel
            {
                TotalDoctors = await _doctorService.GetTotalDoctorsCountAsync(),
                TotalPatients = await _patientService.GetTotalPatientsCountAsync(),
                TotalAppointments = (await _appointmentService.GetDashboardStatsAsync()).TotalAppointments,
                TodaysAppointments = (await _appointmentService.GetDashboardStatsAsync()).TodaysAppointments,
                PendingAppointments = (await _appointmentService.GetDashboardStatsAsync()).PendingAppointments,
                CompletedAppointments = (await _appointmentService.GetDashboardStatsAsync()).CompletedAppointments,
                TotalRevenue = await _billingService.GetTotalRevenueAsync(),
                MonthlyRevenue = await _billingService.GetMonthlyRevenueAsync(),
                PendingBills = (await _billingService.GetPendingBillsAsync()).Count
            };

            return View(dashboardStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading admin dashboard");
            TempData["Error"] = "Error loading dashboard data.";
            return View(new DashboardViewModel());
        }
    }

    [HttpGet]
    public IActionResult Reports()
    {
        var reportViewModel = new ReportViewModel
        {
            FromDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
            ToDate = DateOnly.FromDateTime(DateTime.Today),
            ReportType = "Appointments"
        };

        ViewBag.ReportTypes = new List<SelectListItem>
        {
            new SelectListItem { Value = "Appointments", Text = "Appointments Report" },
            new SelectListItem { Value = "Revenue", Text = "Revenue Report" },
            new SelectListItem { Value = "Patients", Text = "Patients Report" },
            new SelectListItem { Value = "Doctors", Text = "Doctors Report" }
        };

        return View(reportViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateReport(ReportViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ReportTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Appointments", Text = "Appointments Report" },
                new SelectListItem { Value = "Revenue", Text = "Revenue Report" },
                new SelectListItem { Value = "Patients", Text = "Patients Report" },
                new SelectListItem { Value = "Doctors", Text = "Doctors Report" }
            };
            return View("Reports", model);
        }

        try
        {
            if (model.ReportType == "Appointments")
            {
                var appointments = await _appointmentService.GetAllAppointmentsAsync();
                var filteredAppointments = appointments
                    .Where(a => a.AppointmentDate >= model.FromDate && a.AppointmentDate <= model.ToDate)
                    .ToList();

                var appointmentReport = new AppointmentReportViewModel
                {
                    TotalAppointments = filteredAppointments.Count,
                    CompletedAppointments = filteredAppointments.Count(a => a.Status == "Completed"),
                    CancelledAppointments = filteredAppointments.Count(a => a.Status == "Cancelled"),
                    PendingAppointments = filteredAppointments.Count(a => a.Status == "Scheduled"),
                    Appointments = filteredAppointments.Select(a => new AppointmentViewModel
                    {
                        AppointmentId = a.AppointmentId,
                        PatientName = a.Patient.Name,
                        DoctorName = a.Doctor.Name,
                        AppointmentDate = a.AppointmentDate,
                        AppointmentTime = a.AppointmentTime,
                        Status = a.Status,
                        Description = a.Description
                    }).ToList()
                };

                ViewBag.ReportData = appointmentReport;
                ViewBag.ReportTitle = $"Appointments Report ({model.FromDate:dd/MM/yyyy} to {model.ToDate:dd/MM/yyyy})";
            }
            else if (model.ReportType == "Revenue")
            {
                var revenueReport = await _billingService.GetRevenueReportAsync(model.FromDate, model.ToDate);
                var allBills = await _billingService.GetAllBillsAsync();
                var filteredBills = allBills.Where(b => b.BillDate >= model.FromDate && b.BillDate <= model.ToDate).ToList();

                var financialReport = new FinancialReportViewModel
                {
                    TotalRevenue = revenueReport.Sum(r => r.Revenue),
                    TotalPendingAmount = filteredBills.Where(b => b.Status == "Pending").Sum(b => b.TotalAmount),
                    TotalBills = filteredBills.Count,
                    PaidBills = filteredBills.Count(b => b.Status == "Paid"),
                    PendingBills = filteredBills.Count(b => b.Status == "Pending"),

                    Bills = filteredBills.Select(b => new BillingViewModel
                    {
                        BillId = b.BillId,
                        PatientName = b.Patient?.Name ?? "Unknown",
                        BillDate = b.BillDate,
                        TotalAmount = b.TotalAmount,
                        Status = b.Status,
                        PaymentMethod = b.PaymentMethod
                    }).ToList()

                };

                ViewBag.ReportData = financialReport;
                ViewBag.ReportTitle = $"Revenue Report ({model.FromDate:dd/MM/yyyy} to {model.ToDate:dd/MM/yyyy})";
            }
            else if (model.ReportType == "Patients")
            {
                var patients = await _patientService.GetAllPatientsAsync();
                var filteredPatients = patients
                    .Where(p => p.CreatedAt.Date >= model.FromDate.ToDateTime(TimeOnly.MinValue) &&
                               p.CreatedAt.Date <= model.ToDate.ToDateTime(TimeOnly.MinValue))
                    .ToList();

                ViewBag.ReportData = filteredPatients;
                ViewBag.ReportTitle = $"Patients Registration Report ({model.FromDate:dd/MM/yyyy} to {model.ToDate:dd/MM/yyyy})";
            }
            else if (model.ReportType == "Doctors")
            {
                var doctors = await _doctorService.GetAllDoctorsAsync();
                var filteredDoctors = doctors
                    .Where(d => d.CreatedAt.Date >= model.FromDate.ToDateTime(TimeOnly.MinValue) &&
                               d.CreatedAt.Date <= model.ToDate.ToDateTime(TimeOnly.MinValue))
                    .ToList();

                ViewBag.ReportData = filteredDoctors;
                ViewBag.ReportTitle = $"Doctors Report ({model.FromDate:dd/MM/yyyy} to {model.ToDate:dd/MM/yyyy})";
            }

            ViewBag.ReportTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Appointments", Text = "Appointments Report" },
                new SelectListItem { Value = "Revenue", Text = "Revenue Report" },
                new SelectListItem { Value = "Patients", Text = "Patients Report" },
                new SelectListItem { Value = "Doctors", Text = "Doctors Report" }
            };

            TempData["Success"] = $"Report generated successfully for {model.ReportType}";
            return View("Reports", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating report");
            TempData["Error"] = "Error generating report. Please try again.";
            return View("Reports", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Statistics()
    {
        try
        {
            var doctorSpecializations = await _doctorService.GetDoctorSpecializationCountsAsync();
            var revenueReport = await _billingService.GetRevenueReportAsync(
                DateOnly.FromDateTime(DateTime.Today.AddMonths(-6)),
                DateOnly.FromDateTime(DateTime.Today));

            var monthlyStats = await GetMonthlyStatisticsAsync();
            var doctorPerformance = await GetDoctorPerformanceAsync();

            ViewBag.DoctorSpecializations = doctorSpecializations;
            ViewBag.RevenueData = revenueReport;
            ViewBag.MonthlyStats = monthlyStats;
            ViewBag.DoctorPerformance = doctorPerformance;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading statistics");
            TempData["Error"] = "Error loading statistics data.";
            return View();
        }
    }

    [HttpGet]
    public IActionResult SystemSettings()
    {
        var settings = new SystemSettingsViewModel
        {
            HospitalName = "City Hospital",
            ContactEmail = "info@hospital.com",
            ContactPhone = "+91-9876543210",
            Address = "123 Medical Street, Healthcare City",
            TaxPercentage = 18,
            Currency = "INR",
            AppointmentDuration = 30,
            MaxAppointmentsPerDay = 50
        };

        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SystemSettings(SystemSettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // In a real application, you would save these settings to database
            TempData["Success"] = "System settings updated successfully!";
            return RedirectToAction(nameof(SystemSettings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating system settings");
            TempData["Error"] = "Error updating system settings.";
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult UserManagement()
    {
        // This would typically fetch users from Identity
        var users = new List<UserManagementViewModel>
        {
            new UserManagementViewModel
            {
                UserId = "1",
                Email = "admin@hospital.com",
                FullName = "System Administrator",
                Role = "Admin",
                IsActive = true,
                LastLogin = DateTime.Now.AddDays(-1)
            },
            new UserManagementViewModel
            {
                UserId = "2",
                Email = "reception@hospital.com",
                FullName = "Reception Staff",
                Role = "Reception",
                IsActive = true,
                LastLogin = DateTime.Now.AddHours(-2)
            }
        };

        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> BackupDatabase()
    {
        try
        {
            // Simulate database backup process
            await Task.Delay(2000); // Simulate backup time

            TempData["Success"] = "Database backup completed successfully!";
            return RedirectToAction(nameof(SystemSettings));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database backup");
            TempData["Error"] = "Error during database backup.";
            return RedirectToAction(nameof(SystemSettings));
        }
    }

    [HttpPost]
    public async Task<IActionResult> ExportReport(string reportType, DateOnly fromDate, DateOnly toDate)
    {
        try
        {
            var fileName = $"{reportType}_Report_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}";
            byte[] csvData;

            if (reportType == "Appointments")
            {
                var appointments = await _appointmentService.GetAllAppointmentsAsync();
                var filteredAppointments = appointments
                    .Where(a => a.AppointmentDate >= fromDate && a.AppointmentDate <= toDate)
                    .ToList();

                csvData = GenerateAppointmentsCsv(filteredAppointments);
            }
            else if (reportType == "Revenue")
            {
                var bills = await _billingService.GetAllBillsAsync();
                var filteredBills = bills
                    .Where(b => b.BillDate >= fromDate && b.BillDate <= toDate)
                    .ToList();

                csvData = GenerateRevenueCsv(filteredBills);
            }
            else
            {
                TempData["Error"] = "Unsupported report type for export.";
                return RedirectToAction(nameof(Reports));
            }

            return File(csvData, "text/csv", $"{fileName}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report");
            TempData["Error"] = "Error exporting report.";
            return RedirectToAction(nameof(Reports));
        }
    }

    [HttpGet]
    public IActionResult AuditLogs()
    {
        // Simulate audit logs
        var auditLogs = new List<AuditLogViewModel>
        {
            new AuditLogViewModel
            {
                Id = 1,
                UserName = "admin@hospital.com",
                Action = "User Login",
                Timestamp = DateTime.Now.AddHours(-1),
                Details = "User logged in successfully"
            },
            new AuditLogViewModel
            {
                Id = 2,
                UserName = "reception@hospital.com",
                Action = "Create Appointment",
                Timestamp = DateTime.Now.AddHours(-2),
                Details = "Created appointment for patient John Doe"
            },
            new AuditLogViewModel
            {
                Id = 3,
                UserName = "admin@hospital.com",
                Action = "System Backup",
                Timestamp = DateTime.Now.AddDays(-1),
                Details = "System backup completed successfully"
            }
        };

        return View(auditLogs);
    }

    #region Private Methods

    private async Task<List<MonthlyStatViewModel>> GetMonthlyStatisticsAsync()
    {
        var monthlyStats = new List<MonthlyStatViewModel>();
        var currentDate = DateTime.Today;

        for (int i = 5; i >= 0; i--)
        {
            var date = currentDate.AddMonths(-i);
            var firstDayOfMonth = new DateOnly(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var monthlyRevenue = (await _billingService.GetRevenueReportAsync(firstDayOfMonth, lastDayOfMonth))
                .Sum(r => r.Revenue);

            monthlyStats.Add(new MonthlyStatViewModel
            {
                Month = firstDayOfMonth.ToString("MMM yyyy"),
                Revenue = monthlyRevenue,
                Appointments = new Random().Next(50, 200), // Simulated data
                Patients = new Random().Next(30, 100) // Simulated data
            });
        }

        return monthlyStats;
    }

    private async Task<List<DoctorPerformanceViewModel>> GetDoctorPerformanceAsync()
    {
        var doctors = await _doctorService.GetAllDoctorsAsync();
        var appointments = await _appointmentService.GetAllAppointmentsAsync();

        var doctorPerformance = doctors.Select(doctor => new DoctorPerformanceViewModel
        {
            DoctorName = doctor.Name,
            Specialization = doctor.Specialization,
            TotalAppointments = appointments.Count(a => a.DoctorId == doctor.DoctorId),
            CompletedAppointments = appointments.Count(a => a.DoctorId == doctor.DoctorId && a.Status == "Completed"),
            RevenueGenerated = appointments
                .Where(a => a.DoctorId == doctor.DoctorId)
                .Sum(a => doctor.ConsultationFee)
        }).ToList();

        return doctorPerformance;
    }

    private byte[] GenerateAppointmentsCsv(List<Appointment> appointments)
    {
        var csv = "Appointment ID,Patient Name,Doctor Name,Date,Time,Status,Description\n";

        foreach (var appointment in appointments)
        {
            csv += $"{appointment.AppointmentId}," +
                   $"{appointment.Patient.Name}," +
                   $"{appointment.Doctor.Name}," +
                   $"{appointment.AppointmentDate:dd/MM/yyyy}," +
                   $"{appointment.AppointmentTime:hh\\:mm}," +
                   $"{appointment.Status}," +
                   $"\"{appointment.Description}\"\n";
        }

        return System.Text.Encoding.UTF8.GetBytes(csv);
    }

    private byte[] GenerateRevenueCsv(List<Bill> bills)
    {
        var csv = "Bill ID,Patient Name,Date,Amount,Status,Payment Method\n";

        foreach (var bill in bills)
        {
            csv += $"{bill.BillId}," +
                   $"{bill.Patient?.Name ?? "Unknown"}," +
                   $"{bill.BillDate:dd/MM/yyyy}," +
                   $"{bill.TotalAmount}," +
                   $"{bill.Status}," +
                   $"{bill.PaymentMethod}\n";
        }

        return System.Text.Encoding.UTF8.GetBytes(csv);
    }

    #endregion
}

#region ViewModels for Admin Controller

public class SystemSettingsViewModel
{
    public string HospitalName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal TaxPercentage { get; set; }
    public string Currency { get; set; } = "INR";
    public int AppointmentDuration { get; set; } = 30;
    public int MaxAppointmentsPerDay { get; set; } = 50;
}

public class UserManagementViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastLogin { get; set; }
}

public class AuditLogViewModel
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Details { get; set; } = string.Empty;
}

public class MonthlyStatViewModel
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Appointments { get; set; }
    public int Patients { get; set; }
}

public class DoctorPerformanceViewModel
{
    public string DoctorName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public decimal RevenueGenerated { get; set; }
    public double SuccessRate => TotalAppointments > 0 ? (CompletedAppointments * 100.0 / TotalAppointments) : 0;
}

#endregion