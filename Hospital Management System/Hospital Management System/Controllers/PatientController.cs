using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Interfaces;

namespace HospitalManagementSystem.Controllers;

public class PatientController : Controller
{
    private readonly IPatientService _patientService;
    private readonly ILogger<PatientController> _logger;

    public PatientController(IPatientService patientService, ILogger<PatientController> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var patients = await _patientService.GetAllPatientsAsync();
        return View(patients);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Patient patient)
    {
        if (!ModelState.IsValid)
            return View(patient);

        try
        {
            await _patientService.CreatePatientAsync(patient);
            TempData["Success"] = "Patient registered successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient");
            ModelState.AddModelError("", "Error creating patient. Please try again.");
            return View(patient);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);
        if (patient is null)
        {
            return NotFound();
        }
        return View(patient);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);
        if (patient is null)
        {
            return NotFound();
        }
        return View(patient);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Patient patient)
    {
        if (!ModelState.IsValid)
            return View(patient);

        var result = await _patientService.UpdatePatientAsync(patient);
        if (result is null)
        {
            return NotFound();
        }

        TempData["Success"] = "Patient updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);
        if (patient is null)
        {
            return NotFound();
        }
        return View(patient);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var success = await _patientService.DeletePatientAsync(id);
        if (!success)
        {
            return NotFound();
        }

        TempData["Success"] = "Patient deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Search(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return RedirectToAction(nameof(Index));
        }

        var patients = await _patientService.SearchPatientsAsync(searchTerm);
        ViewBag.SearchTerm = searchTerm;
        return View(patients);
    }
}