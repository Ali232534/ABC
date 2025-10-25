using HospitalManagementSystem.Models;
using HospitalManagementSystem.ViewModels;


namespace HospitalManagementSystem.Services.Interfaces;

public interface IDoctorService
{
    Task<List<Doctor>> GetAllDoctorsAsync();
    Task<Doctor?> GetDoctorByIdAsync(int id);
    Task<Doctor> CreateDoctorAsync(Doctor doctor);
    Task<Doctor?> UpdateDoctorAsync(Doctor doctor);
    Task<bool> DeleteDoctorAsync(int id);
    Task<List<Doctor>> GetAvailableDoctorsAsync();
    Task<List<Doctor>> GetDoctorsBySpecializationAsync(string specialization);
    Task<int> GetTotalDoctorsCountAsync();


    // ✅ Must match exactly this signature and return type
    Task<List<SpecializationCountViewModel>> GetDoctorSpecializationCountsAsync();

}