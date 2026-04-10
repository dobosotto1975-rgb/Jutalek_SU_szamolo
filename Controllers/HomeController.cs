using System.Diagnostics;
using AdvisorDashboardApp.Data;
using AdvisorDashboardApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdvisorDashboardApp.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? year, int? month)
    {
        var now = DateTime.Now;
        var selectedYear = year ?? now.Year;
        var selectedMonth = month ?? now.Month;

        var advisors = await _context.Advisors
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();

        var filteredReportsQuery = _context.MonthlyReports
            .AsNoTracking()
            .Where(x => x.Year == selectedYear && x.Month == selectedMonth);

        var latestReports = await filteredReportsQuery
            .Include(x => x.Advisor)
            .OrderByDescending(x => x.Id)
            .Take(12)
            .ToListAsync();

        var monthlyGrouped = await filteredReportsQuery
            .GroupBy(x => x.AdvisorId)
            .Select(g => new
            {
                AdvisorId = g.Key,
                MonthlyAmount = g.Sum(x => x.Amount),
                MonthlySu = g.Sum(x => x.Su),
                BaseCommission = g.Sum(x => x.Commission)
            })
            .ToListAsync();

        var monthlyGroupedDict = monthlyGrouped.ToDictionary(x => x.AdvisorId, x => x);

        var advisorRows = advisors
            .Select(advisor =>
            {
                monthlyGroupedDict.TryGetValue(advisor.Id, out var data);

                var monthlyAmount = data?.MonthlyAmount ?? 0m;
                var monthlySu = data?.MonthlySu ?? 0m;
                var baseCommission = data?.BaseCommission ?? 0m;

                decimal bonusAmount = 0m;

                if (monthlySu >= 9m)
                {
                    bonusAmount = Math.Round(baseCommission * 0.45m, 0);
                }
                else if (monthlySu >= 4.5m)
                {
                    bonusAmount = Math.Round(baseCommission * 0.20m, 0);
                }

                var finalCommission = baseCommission + bonusAmount;

                return new AdvisorDashboardRow
                {
                    AdvisorId = advisor.Id,
                    AdvisorName = advisor.Name,
                    MonthlyAmount = monthlyAmount,
                    MonthlySu = monthlySu,
                    BaseCommission = baseCommission,
                    BonusAmount = bonusAmount,
                    FinalCommission = finalCommission
                };
            })
            .OrderByDescending(x => x.MonthlySu)
            .ThenByDescending(x => x.FinalCommission)
            .ThenBy(x => x.AdvisorName)
            .ToList();

        var allYears = await _context.MonthlyReports
            .AsNoTracking()
            .Select(x => x.Year)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        decimal totalAmount = await _context.MonthlyReports
            .AsNoTracking()
            .SumAsync(x => (decimal?)x.Amount) ?? 0m;

        decimal totalCommission = await _context.MonthlyReports
            .AsNoTracking()
            .SumAsync(x => (decimal?)x.Commission) ?? 0m;

        decimal currentMonthAmount = await filteredReportsQuery
            .SumAsync(x => (decimal?)x.Amount) ?? 0m;

        int monthlyReportCount = await filteredReportsQuery.CountAsync();

        var years = BuildYearList(allYears, selectedYear);
        var months = BuildMonthList(selectedMonth);

        var model = new DashboardViewModel
        {
            SelectedYear = selectedYear,
            SelectedMonth = selectedMonth,
            Years = years,
            Months = months,

            AdvisorCount = advisors.Count,
            MonthlyReportCount = monthlyReportCount,
            TotalAmount = totalAmount,
            TotalCommission = totalCommission,
            CurrentMonthAmount = currentMonthAmount,
            LatestReports = latestReports,
            AdvisorRows = advisorRows
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    private static List<SelectListItem> BuildYearList(List<int> yearsFromData, int selectedYear)
    {
        if (!yearsFromData.Any())
        {
            yearsFromData = Enumerable.Range(DateTime.Now.Year - 2, 6).ToList();
        }
        else
        {
            var minYear = Math.Min(yearsFromData.Min(), DateTime.Now.Year - 1);
            var maxYear = Math.Max(yearsFromData.Max(), DateTime.Now.Year + 1);
            yearsFromData = Enumerable.Range(minYear, maxYear - minYear + 1).ToList();
        }

        return yearsFromData
            .Select(y => new SelectListItem
            {
                Value = y.ToString(),
                Text = y.ToString(),
                Selected = y == selectedYear
            })
            .ToList();
    }

    private static List<SelectListItem> BuildMonthList(int selectedMonth)
    {
        var monthNames = new Dictionary<int, string>
        {
            [1] = "Január",
            [2] = "Február",
            [3] = "Március",
            [4] = "Április",
            [5] = "Május",
            [6] = "Június",
            [7] = "Július",
            [8] = "Augusztus",
            [9] = "Szeptember",
            [10] = "Október",
            [11] = "November",
            [12] = "December"
        };

        return monthNames
            .Select(x => new SelectListItem
            {
                Value = x.Key.ToString(),
                Text = x.Value,
                Selected = x.Key == selectedMonth
            })
            .ToList();
    }
}