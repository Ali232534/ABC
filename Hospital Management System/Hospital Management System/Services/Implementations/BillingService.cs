using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Data;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services.Interfaces;
using HospitalManagementSystem.ViewModels;

namespace HospitalManagementSystem.Services.Implementations;

public class BillingService : IBillingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BillingService> _logger;

    public BillingService(ApplicationDbContext context, ILogger<BillingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Bill>> GetAllBillsAsync()
    {
        return await _context.Bills
            .Include(b => b.Patient)
            .Include(b => b.Appointment)
            .OrderByDescending(b => b.BillDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Bill?> GetBillByIdAsync(int id)
    {
        return await _context.Bills
            .Include(b => b.Patient)
            .Include(b => b.Appointment)
            .FirstOrDefaultAsync(b => b.BillId == id);
    }

    public async Task<Bill> CreateBillAsync(Bill bill)
    {
        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();
        _logger.LogInformation("New bill created for patient: {PatientId}", bill.PatientId);
        return bill;
    }

    public async Task<Bill?> UpdateBillAsync(Bill bill)
    {
        var existingBill = await _context.Bills
            .FirstOrDefaultAsync(b => b.BillId == bill.BillId);

        if (existingBill is null)
            return null;

        _context.Entry(existingBill).CurrentValues.SetValues(bill);
        await _context.SaveChangesAsync();
        return existingBill;
    }

    public async Task<bool> DeleteBillAsync(int id)
    {
        var bill = await _context.Bills.FindAsync(id);
        if (bill is null)
            return false;

        _context.Bills.Remove(bill);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Bill>> GetBillsByPatientAsync(int patientId)
    {
        return await _context.Bills
            .Include(b => b.Appointment)
            .Where(b => b.PatientId == patientId)
            .OrderByDescending(b => b.BillDate)
            .ToListAsync();
    }

    public async Task<List<Bill>> GetPendingBillsAsync()
    {
        return await _context.Bills
            .Include(b => b.Patient)
            .Where(b => b.Status == "Pending")
            .OrderBy(b => b.BillDate)
            .ToListAsync();
    }

    public async Task<Bill?> MarkBillAsPaidAsync(int billId, string paymentMethod)
    {
        var bill = await _context.Bills.FindAsync(billId);
        if (bill is null)
            return null;

        bill.Status = "Paid";
        bill.PaymentMethod = paymentMethod;
        bill.PaidDate = DateTime.Today; // ✅ Fixed conversion issue

        await _context.SaveChangesAsync();
        return bill;
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await _context.Bills
            .Where(b => b.Status == "Paid")
            .SumAsync(b => b.TotalAmount);
    }

    public async Task<decimal> GetMonthlyRevenueAsync()
    {
        var firstDayOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        return await _context.Bills
            .Where(b => b.Status == "Paid" && b.PaidDate >= firstDayOfMonth)
            .SumAsync(b => b.TotalAmount);
    }

    public async Task<List<RevenueReportViewModel>> GetRevenueReportAsync(DateOnly fromDate, DateOnly toDate)
    {
        var from = fromDate.ToDateTime(TimeOnly.MinValue);
        var to = toDate.ToDateTime(TimeOnly.MaxValue);

        return await _context.Bills
            .Where(b => b.Status == "Paid" && b.PaidDate >= from && b.PaidDate <= to)
            .GroupBy(b => b.PaidDate!.Value.Date)
            .Select(g => new RevenueReportViewModel
            {
                Date = DateOnly.FromDateTime(g.Key),
                Revenue = g.Sum(b => b.TotalAmount)
            })
            .OrderBy(r => r.Date)
            .ToListAsync();
    }
}
