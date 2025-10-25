namespace HospitalManagementSystem.ViewModels
{
    public class PatientViewModel
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string BloodGroup { get; set; } = string.Empty;
        public string EmergencyContact { get; set; } = string.Empty;
        public string MedicalHistory { get; set; } = string.Empty;

        // ✅ Now Age can be both calculated or manually set (for controller)
        private int? _age;
        public int Age
        {
            get => _age ?? CalculateAge();
            set => _age = value;
        }

        public int TotalAppointments { get; set; }
        public int TotalBills { get; set; }
        public decimal TotalAmountPaid { get; set; }

        private int CalculateAge()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth > today.AddYears(-age)) age--;
            return age;
        }
    }
}
