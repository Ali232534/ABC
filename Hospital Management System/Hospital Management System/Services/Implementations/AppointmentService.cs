using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.ViewModels;

namespace HospitalManagementSystem.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(ApplicationDbContext context, ILogger<AppointmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            _logger.LogInformation("New appointment created for patient: {PatientId}", appointment.PatientId);
            return appointment;
        }

        public async Task<Appointment?> UpdateAppointmentAsync(Appointment appointment)
        {
            var existingAppointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == appointment.AppointmentId);

            if (existingAppointment is null)
                return null;

            appointment.UpdatedAt = DateTime.UtcNow;
            _context.Entry(existingAppointment).CurrentValues.SetValues(appointment);
            await _context.SaveChangesAsync();
            return existingAppointment;
        }

        public async Task<bool> DeleteAppointmentAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment is null)
                return false;

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Appointment>> GetAppointmentsByDoctorAsync(int doctorId)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsByPatientAsync(int patientId)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetTodaysAppointmentsAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.AppointmentDate == today)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetUpcomingAppointmentsAsync(int days = 7)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var futureDate = today.AddDays(days);

            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.AppointmentDate >= today && a.AppointmentDate <= futureDate)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateOnly date, TimeOnly time)
        {
            return !await _context.Appointments
                .AnyAsync(a => a.DoctorId == doctorId &&
                              a.AppointmentDate == date &&
                              a.AppointmentTime == time &&
                              a.Status != "Cancelled");
        }

        // ✅ Single valid implementation — matches interface
        public async Task<DashboardViewModel> GetDashboardStatsAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var totalAppointments = await _context.Appointments.CountAsync();
            var todaysAppointments = await _context.Appointments
                .CountAsync(a => a.AppointmentDate == today);
            var pendingAppointments = await _context.Appointments
                .CountAsync(a => a.Status == "Scheduled");
            var completedAppointments = await _context.Appointments
                .CountAsync(a => a.Status == "Completed");

            return new DashboardViewModel
            {
                TotalAppointments = totalAppointments,
                TodaysAppointments = todaysAppointments,
                PendingAppointments = pendingAppointments,
                CompletedAppointments = completedAppointments
            };
        }

        Task<DashboardViewModel> IAppointmentService.GetDashboardStatsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Appointment>> GetAppointmentsByStatusAsync(string status)
        {
            throw new NotImplementedException();
        }
    }
}
