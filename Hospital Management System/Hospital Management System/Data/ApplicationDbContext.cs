using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Bill> Bills { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Doctor configuration
        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasIndex(d => d.Email).IsUnique();
            entity.HasIndex(d => d.Phone).IsUnique();
            entity.Property(d => d.ConsultationFee).HasPrecision(18, 2); // Decimal fix
        });

        // Patient configuration
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasIndex(p => p.Email).IsUnique();
            entity.HasIndex(p => p.Phone).IsUnique();
        });

        // Appointment configuration
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasOne(a => a.Patient)
                  .WithMany(p => p.Appointments)
                  .HasForeignKey(a => a.PatientId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Doctor)
                  .WithMany(d => d.Appointments)
                  .HasForeignKey(a => a.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(a => new { a.DoctorId, a.AppointmentDate, a.AppointmentTime })
                  .IsUnique();
        });

        // Bill configuration
        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasOne(b => b.Patient)
                  .WithMany(p => p.Bills)
                  .HasForeignKey(b => b.PatientId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Appointment)
                  .WithMany()
                  .HasForeignKey(b => b.AppointmentId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Decimal properties fix
            entity.Property(b => b.Amount).HasPrecision(18, 2);
            entity.Property(b => b.ConsultationFee).HasPrecision(18, 2);
            entity.Property(b => b.MedicineCharges).HasPrecision(18, 2);
            entity.Property(b => b.RoomCharges).HasPrecision(18, 2);
            entity.Property(b => b.OtherCharges).HasPrecision(18, 2);
            entity.Property(b => b.Discount).HasPrecision(18, 2);
            entity.Property(b => b.TaxAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Doctor>().HasData(
       new Doctor
       {
           DoctorId = 1,
           Name = "Dr. Rajesh Kumar",
           Specialization = "Cardiology",
           Phone = "9876543210",
           Email = "dr.rajesh@hospital.com",
           AvailableFrom = TimeOnly.Parse("09:00"),
           AvailableTo = TimeOnly.Parse("17:00"),
           IsAvailable = true,
           Address = "Cardiology Department, City Hospital",
           ConsultationFee = 800,
           ExperienceYears = 15,
           Qualification = "MD, DM Cardiology",
           CreatedAt = new DateTime(2025, 10, 10) // **static value**
       },
       new Doctor
       {
           DoctorId = 2,
           Name = "Dr. Priya Sharma",
           Specialization = "Pediatrics",
           Phone = "9876543211",
           Email = "dr.priya@hospital.com",
           AvailableFrom = TimeOnly.Parse("10:00"),
           AvailableTo = TimeOnly.Parse("18:00"),
           IsAvailable = true,
           Address = "Pediatrics Department, City Hospital",
           ConsultationFee = 600,
           ExperienceYears = 10,
           Qualification = "MD Pediatrics",
           CreatedAt = new DateTime(2025, 10, 10) // **static value**
       }
   );

    }
}
