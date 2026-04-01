using AdvisorDashboardApp.Data;
using AdvisorDashboardApp.Models;
using AdvisorDashboardApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdvisorDashboardApp.Controllers;

public class MonthlyReportsController : Controller
{
    private readonly AppDbContext _context;
    private readonly IProductCalculationService _productCalculationService;

    public MonthlyReportsController(
        AppDbContext context,
        IProductCalculationService productCalculationService)
    {
        _context = context;
        _productCalculationService = productCalculationService;
    }

    public async Task<IActionResult> Index()
    {
        var reports = await _context.MonthlyReports
            .Include(x => x.Advisor)
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ThenBy(x => x.Advisor!.Name)
            .ToListAsync();

        return View(reports);
    }

    public async Task<IActionResult> Create()
    {
        var model = new MonthlyReport
        {
            Year = DateTime.Now.Year,
            Month = DateTime.Now.Month
        };

        await LoadDropdownsAsync(model.AdvisorId, model.Year, model.Month, model.Product);
        LoadRuleJson();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MonthlyReport report)
    {
        if (!await _context.Advisors.AnyAsync())
        {
            ModelState.AddModelError("", "Előbb vegyél fel legalább egy tanácsadót.");
        }

        report.Product = (report.Product ?? string.Empty).Trim();
        CalculateValues(report);

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(report.AdvisorId, report.Year, report.Month, report.Product);
            LoadRuleJson();
            return View(report);
        }

        _context.MonthlyReports.Add(report);
        await _context.SaveChangesAsync();

        TempData["Success"] = "A havi adat sikeresen rögzítve.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var report = await _context.MonthlyReports.FirstOrDefaultAsync(x => x.Id == id);
        if (report == null)
            return NotFound();

        await LoadDropdownsAsync(report.AdvisorId, report.Year, report.Month, report.Product);
        LoadRuleJson();

        return View(report);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MonthlyReport report)
    {
        if (id != report.Id)
            return NotFound();

        if (!await _context.Advisors.AnyAsync())
        {
            ModelState.AddModelError("", "Előbb vegyél fel legalább egy tanácsadót.");
        }

        report.Product = (report.Product ?? string.Empty).Trim();
        CalculateValues(report);

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(report.AdvisorId, report.Year, report.Month, report.Product);
            LoadRuleJson();
            return View(report);
        }

        var existingReport = await _context.MonthlyReports.FirstOrDefaultAsync(x => x.Id == id);
        if (existingReport == null)
            return NotFound();

        existingReport.AdvisorId = report.AdvisorId;
        existingReport.Year = report.Year;
        existingReport.Month = report.Month;
        existingReport.Product = report.Product;
        existingReport.Amount = report.Amount;
        existingReport.CommissionPercent = report.CommissionPercent;
        existingReport.Divider = report.Divider;
        existingReport.Commission = report.Commission;
        existingReport.Su = report.Su;
        existingReport.IsUkContract = report.IsUkContract;
        existingReport.Notes = report.Notes;

        await _context.SaveChangesAsync();

        TempData["Success"] = "A havi adat sikeresen módosítva lett.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var report = await _context.MonthlyReports
            .Include(x => x.Advisor)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (report == null)
            return NotFound();

        return View(report);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var report = await _context.MonthlyReports.FindAsync(id);

        if (report == null)
            return NotFound();

        _context.MonthlyReports.Remove(report);
        await _context.SaveChangesAsync();

        TempData["Success"] = "A havi adat törölve lett.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Summary()
    {
        var reports = await _context.MonthlyReports
            .Include(x => x.Advisor)
            .ToListAsync();

        var rows = reports
            .GroupBy(x => x.Advisor?.Name ?? "Ismeretlen")
            .Select(g => new MonthlySummaryRow
            {
                AdvisorName = g.Key,
                ReportCount = g.Count(),
                TotalAmount = g.Sum(x => x.Amount),
                TotalCommission = g.Sum(x => x.Commission)
            })
            .OrderByDescending(x => x.TotalAmount)
            .ThenBy(x => x.AdvisorName)
            .ToList();

        var model = new MonthlySummaryViewModel
        {
            Rows = rows,
            GrandTotalAmount = rows.Sum(x => x.TotalAmount),
            GrandTotalCommission = rows.Sum(x => x.TotalCommission)
        };

        return View(model);
    }

    private void CalculateValues(MonthlyReport model)
    {
        var calc = _productCalculationService.Calculate(model.Product, model.Amount, model.IsUkContract);

        model.CommissionPercent = calc.CommissionPercent;
        model.Divider = calc.Divider;
        model.Commission = calc.Commission;
        model.Su = calc.Su;
        model.IsUkContract = calc.IsUkContract;
    }

    private async Task LoadDropdownsAsync(
        int? selectedAdvisorId = null,
        int? selectedYear = null,
        int? selectedMonth = null,
        string? selectedProduct = null)
    {
        var advisors = await _context.Advisors
            .OrderBy(x => x.Name)
            .ToListAsync();

        ViewBag.AdvisorId = new SelectList(advisors, "Id", "Name", selectedAdvisorId);

        var years = new List<SelectListItem>();
        for (int year = DateTime.Now.Year - 2; year <= DateTime.Now.Year + 3; year++)
        {
            years.Add(new SelectListItem
            {
                Value = year.ToString(),
                Text = year.ToString()
            });
        }

        ViewBag.Years = new SelectList(
            years,
            "Value",
            "Text",
            (selectedYear ?? DateTime.Now.Year).ToString());

        var months = new List<SelectListItem>
        {
            new() { Value = "1", Text = "Január" },
            new() { Value = "2", Text = "Február" },
            new() { Value = "3", Text = "Március" },
            new() { Value = "4", Text = "Április" },
            new() { Value = "5", Text = "Május" },
            new() { Value = "6", Text = "Június" },
            new() { Value = "7", Text = "Július" },
            new() { Value = "8", Text = "Augusztus" },
            new() { Value = "9", Text = "Szeptember" },
            new() { Value = "10", Text = "Október" },
            new() { Value = "11", Text = "November" },
            new() { Value = "12", Text = "December" }
        };

        ViewBag.Months = new SelectList(
            months,
            "Value",
            "Text",
            (selectedMonth ?? DateTime.Now.Month).ToString());

        var products = _productCalculationService.GetProducts()
            .Select(x => new SelectListItem
            {
                Value = x,
                Text = x
            })
            .ToList();

        ViewBag.Products = new SelectList(products, "Value", "Text", selectedProduct);
    }

    private void LoadRuleJson()
    {
        ViewBag.ProductRulesJson = _productCalculationService.GetRulesJsonForClient();
    }
}