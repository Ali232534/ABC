using HospitalManagementSystem.Models;
using HospitalManagementSystem.ViewModels;


namespace HospitalManagementSystem.Services.Interfaces;

public interface IBillingService
{
    Task<List<Bill>> GetAllBillsAsync();
    Task<Bill?> GetBillByIdAsync(int id);
    Task<Bill> CreateBillAsync(Bill bill);
    Task<Bill?> UpdateBillAsync(Bill bill);
    Task<bool> DeleteBillAsync(int id);
    Task<List<Bill>> GetBillsByPatientAsync(int patientId);
    Task<List<Bill>> GetPendingBillsAsync();
    Task<Bill?> MarkBillAsPaidAsync(int billId, string paymentMethod);
    Task<decimal> GetTotalRevenueAsync();
    Task<decimal> GetMonthlyRevenueAsync();
   


    // ✅ Correct return type and parameters
    Task<List<RevenueReportViewModel>> GetRevenueReportAsync(DateOnly fromDate, DateOnly toDate);
}