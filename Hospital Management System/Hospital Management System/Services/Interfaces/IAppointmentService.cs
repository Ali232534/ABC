using HospitalManagementSystem.Models;
using HospitalManagementSystem.ViewModels;

namespace HospitalManagementSystem.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<List<Appointment>> GetAllAppointmentsAsync();
        Task<Appointment?> GetAppointmentByIdAsync(int id);
        Task<Appointment> CreateAppointmentAsync(Appointment appointment);
        Task<Appointment?> UpdateAppointmentAsync(Appointment appointment);
        Task<bool> DeleteAppointmentAsync(int id);
        Task<List<Appointment>> GetAppointmentsByDoctorAsync(int doctorId);
        Task<List<Appointment>> GetAppointmentsByPatientAsync(int patientId);
        Task<List<Appointment>> GetTodaysAppointmentsAsync();
        Task<List<Appointment>> GetUpcomingAppointmentsAsync(int days = 7);
        Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateOnly date, TimeOnly time);
        Task<List<Appointment>> GetAppointmentsByStatusAsync(string status);

        // ✅ Added to match implementation
        Task<DashboardViewModel> GetDashboardStatsAsync();
    }
}
