using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.ViewModels;

namespace HospitalManagementSystem.Controllers;

public class HomeController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;
    private readonly IBillingService _billingService;

    public HomeController(
        IAppointmentService appointmentService,
        IDoctorService doctorService,
        IPatientService patientService,
        IBillingService billingService)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _patientService = patientService;
        _billingService = billingService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Dashboard()
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

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}