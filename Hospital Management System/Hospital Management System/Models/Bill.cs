using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Models
{
    public class Bill
    {
        [Key]
        public int BillId { get; set; }

        // ---------- Foreign Keys ----------
        [Required(ErrorMessage = "Patient is required")]
        public int PatientId { get; set; }

        // ✅ Appointment is optional — no [Required]
        public int? AppointmentId { get; set; }

        // ---------- Bill Info ----------
        [Required(ErrorMessage = "Bill date is required")]
        public DateOnly BillDate { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Precision(18, 2)]
        public decimal Amount { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Paid, Cancelled

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        // ---------- Charges ----------
        [Precision(18, 2)]
        public decimal ConsultationFee { get; set; }

        [Precision(18, 2)]
        public decimal MedicineCharges { get; set; }

        [Precision(18, 2)]
        public decimal RoomCharges { get; set; }

        [Precision(18, 2)]
        public decimal OtherCharges { get; set; }

        [Precision(18, 2)]
        public decimal Discount { get; set; }

        [Precision(18, 2)]
        public decimal TaxAmount { get; set; }

        // ✅ Computed total — not mapped to DB
        [NotMapped]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount =>
            (ConsultationFee + MedicineCharges + RoomCharges + OtherCharges - Discount) + TaxAmount;

        // ---------- Payment ----------
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Cash";

        public DateTime? PaidDate { get; set; }

        // ---------- Metadata ----------
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ---------- Navigation ----------
        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }

        [ForeignKey("AppointmentId")]
        public Appointment? Appointment { get; set; }
    }
}
