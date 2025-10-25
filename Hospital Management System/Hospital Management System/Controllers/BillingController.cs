using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.ViewModels;

namespace HospitalManagementSystem.Controllers
{
    public class BillingController : Controller
    {
        private readonly IBillingService _billingService;
        private readonly IPatientService _patientService;
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<BillingController> _logger;

        public BillingController(
            IBillingService billingService,
            IPatientService patientService,
            IAppointmentService appointmentService,
            ILogger<BillingController> logger)
        {
            _billingService = billingService;
            _patientService = patientService;
            _appointmentService = appointmentService;
            _logger = logger;
        }

        // ================== 📄 BILL LIST ==================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var bills = await _billingService.GetAllBillsAsync();
                return View(bills);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading bills");
                TempData["Error"] = "An error occurred while loading bills.";
                return View(new List<Bill>());
            }
        }

        // ================== 🧾 CREATE BILL (GET) ==================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();
                var appointments = await _appointmentService.GetAllAppointmentsAsync();

                var viewModel = new CreateBillViewModel
                {
                    Patients = patients.Select(p => new PatientViewModel
                    {
                        Id = p.PatientId,
                        Name = p.Name,
                        Age = (int)((DateTime.Today - p.DateOfBirth.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.25),
                        Gender = p.Gender,
                        Phone = p.Phone,
                        Email = p.Email
                    }).ToList(),

                    Appointments = appointments.Select(a => new AppointmentViewModel
                    {
                        AppointmentId = a.AppointmentId,
                        DoctorName = a.Doctor?.Name ?? "N/A",
                        PatientName = a.Patient?.Name ?? "N/A",
                        AppointmentDate = a.AppointmentDate,
                        Status = a.Status
                    }).ToList(),

                    BillDate = DateOnly.FromDateTime(DateTime.Today),
                    TaxPercentage = 18
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading bill creation page");
                TempData["Error"] = "An error occurred while loading the bill creation form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ================== 💾 CREATE BILL (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBillViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(viewModel);
                return View(viewModel);
            }

            try
            {
                var subtotal = viewModel.ConsultationFee + viewModel.MedicineCharges +
                               viewModel.RoomCharges + viewModel.OtherCharges - viewModel.Discount;

                var taxAmount = subtotal * (viewModel.TaxPercentage / 100);

                var bill = new Bill
                {
                    PatientId = viewModel.PatientId,
                    AppointmentId = viewModel.AppointmentId,
                    BillDate = viewModel.BillDate,
                    Description = viewModel.Description,
                    ConsultationFee = viewModel.ConsultationFee,
                    MedicineCharges = viewModel.MedicineCharges,
                    RoomCharges = viewModel.RoomCharges,
                    OtherCharges = viewModel.OtherCharges,
                    Discount = viewModel.Discount,
                    TaxAmount = taxAmount,
                    Amount = subtotal + taxAmount,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                await _billingService.CreateBillAsync(bill);

                TempData["Success"] = "Bill created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bill");
                TempData["Error"] = "An error occurred while creating the bill.";
                await LoadDropdownsAsync(viewModel);
                return View(viewModel);
            }
        }

        // ================== 📊 REPORTS ==================
        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            try
            {
                var fromDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
                var toDate = DateOnly.FromDateTime(DateTime.Today);

                var revenueReport = await _billingService.GetRevenueReportAsync(fromDate, toDate);
                var totalRevenue = await _billingService.GetTotalRevenueAsync();
                var monthlyRevenue = await _billingService.GetMonthlyRevenueAsync();
                var allBills = await _billingService.GetAllBillsAsync();

                var viewModel = new FinancialReportViewModel
                {
                    TotalRevenue = totalRevenue,
                    MonthlyRevenue = monthlyRevenue,
                    PaidBills = allBills.Count(b => b.Status == "Paid"),
                    PendingBills = allBills.Count(b => b.Status == "Pending"),
                    RevenueData = revenueReport
                };

                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading billing reports");
                TempData["Error"] = "An error occurred while loading reports.";
                return View(new FinancialReportViewModel());
            }
        }

        // ================== ⚙️ HELPER ==================
        private async Task LoadDropdownsAsync(CreateBillViewModel viewModel)
        {
            var patients = await _patientService.GetAllPatientsAsync();
            var appointments = await _appointmentService.GetAllAppointmentsAsync();

            viewModel.Patients = patients.Select(p => new PatientViewModel
            {
                Id = p.PatientId,
                Name = p.Name,
                Age = (int)((DateTime.Today - p.DateOfBirth.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.25),
                Gender = p.Gender,
                Phone = p.Phone,
                Email = p.Email
            }).ToList();

            viewModel.Appointments = appointments.Select(a => new AppointmentViewModel
            {
                AppointmentId = a.AppointmentId,
                DoctorName = a.Doctor?.Name ?? "N/A",
                PatientName = a.Patient?.Name ?? "N/A",
                AppointmentDate = a.AppointmentDate,
                Status = a.Status
            }).ToList();
        }
    }
}
