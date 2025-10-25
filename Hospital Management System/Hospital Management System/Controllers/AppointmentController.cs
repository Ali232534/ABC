using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers;

public class AppointmentController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;
    private readonly ILogger<AppointmentController> _logger;

    public AppointmentController(
        IAppointmentService appointmentService,
        IDoctorService doctorService,
        IPatientService patientService,
        ILogger<AppointmentController> logger)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _patientService = patientService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync();
        return View(appointments);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var viewModel = new CreateAppointmentViewModel
        {
            AvailableDoctors = (await _doctorService.GetAvailableDoctorsAsync())
                .Select(d => new DoctorViewModel
                {
                    DoctorId = d.DoctorId,
                    Name = d.Name,
                    Specialization = d.Specialization
                }).ToList(),

            Patients = (await _patientService.GetAllPatientsAsync())
                .Select(p => new PatientViewModel
                {
                    PatientId = p.PatientId,
                    Name = p.Name,
                    Gender = p.Gender,
                    DateOfBirth = p.DateOfBirth
                }).ToList(),

            AppointmentDate = DateOnly.FromDateTime(DateTime.Today)
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAppointmentViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await LoadDoctorAndPatientLists(viewModel);
            return View(viewModel);
        }

        // ✅ Check if time slot is free
        var isAvailable = await _appointmentService.IsTimeSlotAvailableAsync(
            viewModel.DoctorId, viewModel.AppointmentDate, viewModel.AppointmentTime);

        if (!isAvailable)
        {
            ModelState.AddModelError("", "Selected time slot is not available. Please choose another time.");
            await LoadDoctorAndPatientLists(viewModel);
            return View(viewModel);
        }

        var appointment = new Appointment
        {
            PatientId = viewModel.PatientId,
            DoctorId = viewModel.DoctorId,
            AppointmentDate = viewModel.AppointmentDate,
            AppointmentTime = viewModel.AppointmentTime,
            Description = viewModel.Description,
            Symptoms = viewModel.Symptoms,
            Status = "Scheduled"
        };

        try
        {
            await _appointmentService.CreateAppointmentAsync(appointment);
            TempData["Success"] = "Appointment scheduled successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            ModelState.AddModelError("", "Error creating appointment. Please try again.");
            await LoadDoctorAndPatientLists(viewModel);
            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null) return NotFound();

        var viewModel = new CreateAppointmentViewModel
        {
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            AppointmentDate = appointment.AppointmentDate,
            AppointmentTime = appointment.AppointmentTime,
            Description = appointment.Description,
            Symptoms = appointment.Symptoms
        };

        await LoadDoctorAndPatientLists(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateAppointmentViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await LoadDoctorAndPatientLists(viewModel);
            return View(viewModel);
        }

        var existingAppointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (existingAppointment == null) return NotFound();

        existingAppointment.PatientId = viewModel.PatientId;
        existingAppointment.DoctorId = viewModel.DoctorId;
        existingAppointment.AppointmentDate = viewModel.AppointmentDate;
        existingAppointment.AppointmentTime = viewModel.AppointmentTime;
        existingAppointment.Description = viewModel.Description;
        existingAppointment.Symptoms = viewModel.Symptoms;

        await _appointmentService.UpdateAppointmentAsync(existingAppointment);
        TempData["Success"] = "Appointment updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Today()
    {
        var appointments = await _appointmentService.GetTodaysAppointmentsAsync();
        return View(appointments);
    }

    [HttpGet]
    public async Task<IActionResult> Upcoming()
    {
        var appointments = await _appointmentService.GetUpcomingAppointmentsAsync();
        return View(appointments);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
        if (appointment == null) return NotFound();

        appointment.Status = status;
        await _appointmentService.UpdateAppointmentAsync(appointment);

        TempData["Success"] = $"Appointment marked as {status}";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ByDoctor(int doctorId)
    {
        var appointments = await _appointmentService.GetAppointmentsByDoctorAsync(doctorId);
        var doctor = await _doctorService.GetDoctorByIdAsync(doctorId);
        ViewBag.DoctorName = doctor?.Name;
        return View(appointments);
    }

    [HttpGet]
    public async Task<IActionResult> ByPatient(int patientId)
    {
        var appointments = await _appointmentService.GetAppointmentsByPatientAsync(patientId);
        var patient = await _patientService.GetPatientByIdAsync(patientId);
        ViewBag.PatientName = patient?.Name;
        return View(appointments);
    }

    // 🔹 Helper Method to Reload Dropdown Data
    private async Task LoadDoctorAndPatientLists(CreateAppointmentViewModel viewModel)
    {
        var doctors = await _doctorService.GetAvailableDoctorsAsync();
        var patients = await _patientService.GetAllPatientsAsync();

        viewModel.AvailableDoctors = doctors.Select(d => new DoctorViewModel
        {
            DoctorId = d.DoctorId,
            Name = d.Name,
            Specialization = d.Specialization
        }).ToList();

        viewModel.Patients = patients.Select(p => new PatientViewModel
        {
            PatientId = p.PatientId,
            Name = p.Name,
            Gender = p.Gender,
            DateOfBirth = p.DateOfBirth
        }).ToList();
    }
}
