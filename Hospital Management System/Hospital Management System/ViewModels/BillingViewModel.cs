namespace HospitalManagementSystem.ViewModels
{
    public class BillingViewModel
    {
        public int BillId { get; set; }

        public int PatientId { get; set; }

        public int? AppointmentId { get; set; }

        public DateOnly BillDate { get; set; }

        public decimal Amount { get; set; }

        public string Status { get; set; } = "Pending";

        public string Description { get; set; } = string.Empty;

        public decimal ConsultationFee { get; set; }

        public decimal MedicineCharges { get; set; }

        public decimal RoomCharges { get; set; }

        public decimal OtherCharges { get; set; }

        public decimal Discount { get; set; }

        public decimal TaxAmount { get; set; }

        // ✅ Made writable to avoid "read-only" assignment error
        public decimal TotalAmount { get; set; }

        public string PaymentMethod { get; set; } = "Cash";

        public DateOnly? PaidDate { get; set; }

        // ---------- Additional Details ----------
        public string PatientName { get; set; } = string.Empty;

        public string PatientPhone { get; set; } = string.Empty;

        public string DoctorName { get; set; } = string.Empty;

        public string AppointmentDate { get; set; } = string.Empty;

        // ✅ Optional helper if you want automatic calculation
        public decimal CalculatedTotal =>
            (ConsultationFee + MedicineCharges + RoomCharges + OtherCharges - Discount) + TaxAmount;
    }

    // ✅ For Bill Creation UI (Dropdowns, etc.)
    public class CreateBillViewModel
    {
        public int PatientId { get; set; }

        public int? AppointmentId { get; set; }

        public DateOnly BillDate { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal ConsultationFee { get; set; }

        public decimal MedicineCharges { get; set; }

        public decimal RoomCharges { get; set; }

        public decimal OtherCharges { get; set; }

        public decimal Discount { get; set; }

        // ✅ Tax percentage with default 18%
        public decimal TaxPercentage { get; set; } = 18;

        // ✅ For dropdown lists
        public List<PatientViewModel>? Patients { get; set; }

        public List<AppointmentViewModel>? Appointments { get; set; }
    }

    // ✅ For Reports (Charts / Analytics)
    public class RevenueReportViewModel
    {
        public DateOnly Date { get; set; }

        public decimal Revenue { get; set; }
    }
}
