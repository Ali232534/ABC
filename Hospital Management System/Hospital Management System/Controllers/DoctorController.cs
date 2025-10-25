using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Interfaces;

namespace HospitalManagementSystem.Controllers;

public class DoctorController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly ILogger<DoctorController> _logger;

    public DoctorController(IDoctorService doctorService, ILogger<DoctorController> logger)
    {
        _doctorService = doctorService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var doctors = await _doctorService.GetAllDoctorsAsync();
        return View(doctors);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Doctor doctor)
    {
        if (!ModelState.IsValid)
            return View(doctor);

        try
        {
            await _doctorService.CreateDoctorAsync(doctor);
            TempData["Success"] = "Doctor created successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating doctor");
            ModelState.AddModelError("", "Error creating doctor. Please try again.");
            return View(doctor);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var doctor = await _doctorService.GetDoctorByIdAsync(id);
        if (doctor is null)
        {
            return NotFound();
        }
        return View(doctor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Doctor doctor)
    {
        if (!ModelState.IsValid)
            return View(doctor);

        var result = await _doctorService.UpdateDoctorAsync(doctor);
        if (result is null)
        {
            return NotFound();
        }

        TempData["Success"] = "Doctor updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var doctor = await _doctorService.GetDoctorByIdAsync(id);
        if (doctor is null)
        {
            return NotFound();
        }
        return View(doctor);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var success = await _doctorService.DeleteDoctorAsync(id);
        if (!success)
        {
            return NotFound();
        }

        TempData["Success"] = "Doctor deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Available()
    {
        var doctors = await _doctorService.GetAvailableDoctorsAsync();
        return View(doctors);
    }

    [HttpGet]
    public async Task<IActionResult> Specialization(string specialization)
    {
        var doctors = await _doctorService.GetDoctorsBySpecializationAsync(specialization);
        ViewBag.Specialization = specialization;
        return View(doctors);
    }
}