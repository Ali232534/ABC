using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Specialization { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public TimeOnly AvailableFrom { get; set; }
        public TimeOnly AvailableTo { get; set; }
        public bool IsAvailable { get; set; } = true;

        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Precision(18, 2)]
        public decimal ConsultationFee { get; set; }

        public int ExperienceYears { get; set; }

        [StringLength(200)]
        public string Qualification { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
