using System.Text.Json;
using AdvisorDashboardApp.Data;
using AdvisorDashboardApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdvisorDashboardApp.Controllers;

public class DataEntryController : Controller
{
    private readonly AppDbContext _context;

    private static readonly Dictionary<string, ProductRule> ProductRules = new()
    {
        ["Vienna Plan, Age alapdíj, ha a teljes díj 300 e ft alatti fé.,éves"] = new(175000m, 0.35m, "percent"),
        ["Vienna Plan, Age alapdíj, ha a teljes díj 300 e ft alatti né."] = new(200000m, 0.35m, "percent"),
        ["Vienna Plan, Age alapdíj, ha a teljes díj 300 e ft alatti havi CSOB"] = new(270000m, 0.35m, "percent"),
        ["Vienna Plan, Age alapdíj, ha a teljes díj 300 e ft alatti havi átutalás, kártya"] = new(480000m, 0.35m, "percent"),

        ["Vienna Plan, Age alapdíj, ha a teljes díj 300 - 310 e ft között fé.,éves"] = new(145000m, 0.35m, "percent"),
        ["Vienna Plan, Age alapdíj, ha a teljes díj 300 - 310 e ft között ft alatti né."] = new(170000m, 0.35m, "percent"),
        ["Vienna Plan, Age alapdíj, ha a teljes díj 300 - 310 e ft között havi CSOB"] = new(220000m, 0.35m, "percent"),
        ["Vienna Plan, Age alapdíj, ha a teljes díj 300 - 310 e ft között havi átutalás, kártya"] = new(400000m, 0.35m, "percent"),

        ["Vienna Plan, Age alapdíj, ha a teljes díj 310 -410 e ft között  fé.,éves"] = new(145000m, 0.40m, "percent"),
        ["Vienna Plan, Age alapdíj, ha a teljes díj 310 -410 e ft között né."] = new(170000m, 0.40m, "percent"),
        ["Vienna Plan, Age alapdíj, ha a teljes díj 310 -410 e ft között havi CSOB"] = new(220000m, 0.40m, "percent"),
        ["Vienna Plan, Age alapdíj, ha a teljes díj 310 -410 e ft között havi átutalás, kártya"] = new(400000m, 0.40m, "percent"),

        ["Vienna Plan, Age alapdíj, ha a teljes díj minimum  410 e ft fé.,éves"] = new(145000m, 0.45m, "percent"),
        ["Vienna Plan, Age alapdíj, ha a teljes díj minimum  410 e ft né."] = new(170000m, 0.45m, "percent"),
        ["Vienna Plan, Age alapdíj, ha a teljes díj minimum  410 e ft havi CSOB"] = new(220000m, 0.45m, "percent"),
        ["Vienna Plan, Age alapdíj, ha a teljes díj minimum  410 e ft havi átutalás, kártya"] = new(400000m, 0.45m, "percent"),

        ["Vienna Plan, Age ÜK"] = new(720000m, 0.00m, "percent"),
        ["Életbiztosítási  (UL, YES) kiegészítők"] = new(45000m, 0.75m, "percent"),
        ["Lakásbiztosítás vagy MFO"] = new(27500m, 0.09m, "percent"),
        ["Vagyon (Business Class, BC; KKV Felelősség/"] = new(80000m, 0.167m, "percent"),
        ["KKV Egészségügyi, Rendezvényszervezői Felelősség"] = new(50000m, 0.167m, "percent"),
        ["KKV Elber, Gépbiztosítás"] = new(80000m, 0.093m, "percent"),
        ["Egyedi vagyon"] = new(150000m, 0.093m, "percent"),
        ["Balesetbiztosítás - Menta"] = new(50000m, 0.25m, "percent"),

        ["Vienna Yes alapdíj, ha a teljes díj  120e Ft-ig /állománydíjas/"] = new(60000m, 0.55m, "percent"),
        ["Vienna Yes alapdíj, ha a teljes díj 120-145e Ft között /állománydíjas/"] = new(60000m, 0.62m, "percent"),
        ["Vienna Yes alapdíj, ha a teljes díj 145e Ft-tól /állománydíjas/"] = new(60000m, 0.70m, "percent"),

        ["Napnyugta /5év, állománydíj/"] = new(45000m, 0.70m, "percent"),
        ["Kompakt csoportos élet és baleset"] = new(50000m, 0.14m, "percent"),
        ["Private-Med Next"] = new(200000m, 0.125m, "percent"),
        ["CASCO"] = new(100000m, 0.09m, "percent"),
        ["KGFB"] = new(350000m, 0.0265m, "percent"),

        ["Utas"] = new(120000m, null, "none"),
        ["Útitárs"] = new(50000m, null, "db"),

        ["Eseti díj"] = new(1000000m, 0.025m, "percent"),
        ["Eseti díj Plan, Age"] = new(1000000m, 0.0112m, "percent"),
        ["Eseti díj ÜK"] = new(1000000m, 0.00m, "percent")
    };

    public DataEntryController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        await LoadDropdownsAsync();
        LoadRuleJson();

        return View(new MonthlyReport
        {
            Year = DateTime.Now.Year,
            Month = DateTime.Now.Month,
            Commission = 0m,
            Su = 0m
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(MonthlyReport model)
    {
        if (!await _context.Advisors.AnyAsync())
        {
            ModelState.AddModelError("", "Előbb vegyél fel legalább egy tanácsadót.");
        }

        CalculateValues(model);

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync(model.AdvisorId, model.Year, model.Month, model.Product);
            LoadRuleJson();
            return View(model);
        }

        _context.MonthlyReports.Add(model);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Az adat sikeresen rögzítve lett.";
        return RedirectToAction(nameof(Index));
    }

    private void CalculateValues(MonthlyReport model)
    {
        model.Commission = 0m;
        model.Su = 0m;

        if (string.IsNullOrWhiteSpace(model.Product))
            return;

        if (!ProductRules.TryGetValue(model.Product, out var rule))
            return;

        if (rule.Divisor > 0)
        {
            model.Su = Math.Round(model.Amount / rule.Divisor, 4);
        }

        if (rule.Mode == "percent" && rule.Percent.HasValue)
        {
            model.Commission = Math.Round(model.Amount * rule.Percent.Value, 0);
        }
        else if (rule.Mode == "none")
        {
            model.Commission = 0m;
        }
        else if (rule.Mode == "db")
        {
            model.Commission = 0m;
        }
    }

    private async Task LoadDropdownsAsync(
        int? selectedAdvisorId = null,
        int? selectedYear = null,
        int? selectedMonth = null,
        string? selectedProduct = null)
    {
        var advisors = await _context.Advisors
            .Where(x => x.IsActive)
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
            (selectedYear ?? DateTime.Now.Year).ToString()
        );

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
            (selectedMonth ?? DateTime.Now.Month).ToString()
        );

        var products = ProductRules.Keys
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
        var jsonReady = ProductRules.ToDictionary(
            x => x.Key,
            x => new
            {
                divisor = x.Value.Divisor,
                percent = x.Value.Percent,
                mode = x.Value.Mode
            });

        ViewBag.ProductRulesJson = JsonSerializer.Serialize(jsonReady);
    }

    private sealed record ProductRule(decimal Divisor, decimal? Percent, string Mode);
}