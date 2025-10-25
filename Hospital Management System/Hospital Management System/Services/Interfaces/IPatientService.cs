using HospitalManagementSystem.Models;


namespace HospitalManagementSystem.Services.Interfaces;

public interface IPatientService
{
    Task<List<Patient>> GetAllPatientsAsync();
    Task<Patient?> GetPatientByIdAsync(int id);
    Task<Patient> CreatePatientAsync(Patient patient);
    Task<Patient?> UpdatePatientAsync(Patient patient);
    Task<bool> DeletePatientAsync(int id);
    Task<List<Patient>> SearchPatientsAsync(string searchTerm);
    Task<int> GetTotalPatientsCountAsync();
    Task<List<Patient>> GetRecentPatientsAsync(int count = 10);
}