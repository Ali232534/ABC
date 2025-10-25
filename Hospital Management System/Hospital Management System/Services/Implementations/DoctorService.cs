using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.ViewModels;

namespace HospitalManagementSystem.Services.Implementations;

public class DoctorService : IDoctorService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DoctorService> _logger;

    public DoctorService(ApplicationDbContext context, ILogger<DoctorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Doctor>> GetAllDoctorsAsync()
    {
        return await _context.Doctors
            .OrderBy(d => d.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Doctor?> GetDoctorByIdAsync(int id)
    {
        return await _context.Doctors
            .FirstOrDefaultAsync(d => d.DoctorId == id);
    }

    public async Task<Doctor> CreateDoctorAsync(Doctor doctor)
    {
        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();
        _logger.LogInformation("New doctor created: {DoctorName}", doctor.Name);
        return doctor;
    }

    public async Task<Doctor?> UpdateDoctorAsync(Doctor doctor)
    {
        var existingDoctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.DoctorId == doctor.DoctorId);

        if (existingDoctor is null)
            return null;

        _context.Entry(existingDoctor).CurrentValues.SetValues(doctor);
        await _context.SaveChangesAsync();
        return existingDoctor;
    }

    public async Task<bool> DeleteDoctorAsync(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor is null)
            return false;

        _context.Doctors.Remove(doctor);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Doctor>> GetAvailableDoctorsAsync()
    {
        return await _context.Doctors
            .Where(d => d.IsAvailable)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<List<Doctor>> GetDoctorsBySpecializationAsync(string specialization)
    {
        return await _context.Doctors
            .Where(d => d.Specialization.Contains(specialization) && d.IsAvailable)
            .ToListAsync();
    }

    public async Task<int> GetTotalDoctorsCountAsync()
    {
        return await _context.Doctors.CountAsync();
    }

    public async Task<List<SpecializationCountViewModel>> GetDoctorSpecializationCountsAsync()
    {
        return await _context.Doctors
            .GroupBy(d => d.Specialization)
            .Select(g => new SpecializationCountViewModel
            {
                Specialization = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
    }

    Task<List<SpecializationCountViewModel>> IDoctorService.GetDoctorSpecializationCountsAsync()
    {
        throw new NotImplementedException();
    }
}