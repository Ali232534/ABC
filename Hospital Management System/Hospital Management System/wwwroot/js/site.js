// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Hospital Management System JavaScript

// Initialize tooltips
document.addEventListener('DOMContentLoaded', function () {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Auto-dismiss alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            if (alert) {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }
        }, 5000);
    });

    // Form validation enhancements
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            const submitButton = this.querySelector('button[type="submit"]');
            if (submitButton) {
                submitButton.disabled = true;
                submitButton.innerHTML = '<span class="loading"></span> Processing...';
            }
        });
    });

    // Real-time amount calculation for billing
    const calculateTotalAmount = () => {
        const consultationFee = parseFloat(document.getElementById('ConsultationFee')?.value) || 0;
        const medicineCharges = parseFloat(document.getElementById('MedicineCharges')?.value) || 0;
        const roomCharges = parseFloat(document.getElementById('RoomCharges')?.value) || 0;
        const otherCharges = parseFloat(document.getElementById('OtherCharges')?.value) || 0;
        const discount = parseFloat(document.getElementById('Discount')?.value) || 0;
        const taxPercentage = parseFloat(document.getElementById('TaxPercentage')?.value) || 0;

        const subtotal = consultationFee + medicineCharges + roomCharges + otherCharges - discount;
        const taxAmount = subtotal * (taxPercentage / 100);
        const totalAmount = subtotal + taxAmount;

        // Update display
        const totalDisplay = document.getElementById('TotalAmountDisplay');
        if (totalDisplay) {
            totalDisplay.textContent = '₹' + totalAmount.toFixed(2);
        }
    };

    // Attach event listeners for amount calculation
    const amountInputs = ['ConsultationFee', 'MedicineCharges', 'RoomCharges', 'OtherCharges', 'Discount', 'TaxPercentage'];
    amountInputs.forEach(id => {
        const element = document.getElementById(id);
        if (element) {
            element.addEventListener('input', calculateTotalAmount);
        }
    });

    // Initialize amount calculation
    calculateTotalAmount();
});

// Search functionality
function searchPatients() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase();
    const patientRows = document.querySelectorAll('#patientsTable tbody tr');

    patientRows.forEach(row => {
        const text = row.textContent.toLowerCase();
        row.style.display = text.includes(searchTerm) ? '' : 'none';
    });
}

// Date and time utilities
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-IN');
}

function formatTime(timeString) {
    const time = new Date('1970-01-01T' + timeString + 'Z');
    return time.toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit' });
}

// Export functionality
function exportToCSV(tableId, filename) {
    const table = document.getElementById(tableId);
    const rows = table.querySelectorAll('tr');
    const csv = [];

    for (let i = 0; i < rows.length; i++) {
        const row = [], cols = rows[i].querySelectorAll('td, th');

        for (let j = 0; j < cols.length; j++) {
            // Clean the text content
            let data = cols[j].innerText.replace(/(\r\n|\n|\r)/gm, '').replace(/(\s\s)/gm, ' ');
            data = data.replace(/"/g, '""');
            row.push('"' + data + '"');
        }

        csv.push(row.join(','));
    }

    // Download CSV file
    const csvString = csv.join('\n');
    const blob = new Blob([csvString], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.setAttribute('hidden', '');
    a.setAttribute('href', url);
    a.setAttribute('download', filename + '.csv');
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
}

// Print functionality
function printElement(elementId) {
    const printContent = document.getElementById(elementId).innerHTML;
    const originalContent = document.body.innerHTML;

    document.body.innerHTML = printContent;
    window.print();
    document.body.innerHTML = originalContent;
    window.location.reload();
}

// Auto-refresh for dashboard
function startAutoRefresh(interval = 30000) {
    setInterval(() => {
        const dashboardElements = document.querySelectorAll('[data-auto-refresh]');
        dashboardElements.forEach(element => {
            // Implement AJAX refresh logic here
            console.log('Auto-refreshing dashboard element:', element);
        });
    }, interval);
}

// Start auto-refresh if on dashboard
if (window.location.pathname.includes('Dashboard')) {
    startAutoRefresh();
}