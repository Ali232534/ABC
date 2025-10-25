using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.ViewModels;

namespace HospitalManagementSystem.Services.Implementations;

public class PatientService : IPatientService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PatientService> _logger;

    public PatientService(ApplicationDbContext context, ILogger<PatientService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Patient>> GetAllPatientsAsync()
    {
        return await _context.Patients
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Patient?> GetPatientByIdAsync(int id)
    {
        return await _context.Patients
            .FirstOrDefaultAsync(p => p.PatientId == id);
    }

    public async Task<Patient> CreatePatientAsync(Patient patient)
    {
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
        _logger.LogInformation("New patient created: {PatientName}", patient.Name);
        return patient;
    }

    public async Task<Patient?> UpdatePatientAsync(Patient patient)
    {
        var existingPatient = await _context.Patients
            .FirstOrDefaultAsync(p => p.PatientId == patient.PatientId);

        if (existingPatient is null)
            return null;

        _context.Entry(existingPatient).CurrentValues.SetValues(patient);
        await _context.SaveChangesAsync();
        return existingPatient;
    }

    public async Task<bool> DeletePatientAsync(int id)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient is null)
            return false;

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Patient>> SearchPatientsAsync(string searchTerm)
    {
        return await _context.Patients
            .Where(p => p.Name.Contains(searchTerm) ||
                       p.Phone.Contains(searchTerm) ||
                       p.Email.Contains(searchTerm))
            .ToListAsync();
    }

    public async Task<int> GetTotalPatientsCountAsync()
    {
        return await _context.Patients.CountAsync();
    }

    public async Task<List<Patient>> GetRecentPatientsAsync(int count = 10)
    {
        return await _context.Patients
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}