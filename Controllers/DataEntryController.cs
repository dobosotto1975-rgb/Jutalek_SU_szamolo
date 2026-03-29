using AdvisorDashboardApp.Data;
using AdvisorDashboardApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdvisorDashboardApp.Controllers;

public class DataEntryController : Controller
{
    private readonly AppDbContext _context;

    public DataEntryController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = await BuildViewModelAsync(new DataEntryViewModel
        {
            Year = DateTime.Now.Year,
            Month = DateTime.Now.Month
        });

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(DataEntryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model = await BuildViewModelAsync(model);
            return View(model);
        }

        var calc = GetProductCalculation(model.Product, model.Amount, model.IsUkContract);

        var report = new MonthlyReport
        {
            AdvisorId = model.AdvisorId,
            Year = model.Year,
            Month = model.Month,
            Product = model.Product,
            Amount = model.Amount,
            CommissionPercent = calc.CommissionPercent,
            Divider = calc.Divider,
            Commission = calc.Commission,
            Su = calc.Su,
            IsUkContract = model.IsUkContract
        };

        _context.MonthlyReports.Add(report);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Az adat sikeresen rögzítve lett.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<DataEntryViewModel> BuildViewModelAsync(DataEntryViewModel model)
    {
        var advisors = await _context.Advisors
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();

        model.Advisors = advisors
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name,
                Selected = x.Id == model.AdvisorId
            })
            .ToList();

        model.Years = Enumerable.Range(DateTime.Now.Year - 2, 6)
            .Select(y => new SelectListItem
            {
                Value = y.ToString(),
                Text = y.ToString(),
                Selected = y == model.Year
            })
            .ToList();

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

        model.Months = monthNames
            .Select(x => new SelectListItem
            {
                Value = x.Key.ToString(),
                Text = x.Value,
                Selected = x.Key == model.Month
            })
            .ToList();

        var products = GetProducts();

        model.Products = products
            .Select(x => new SelectListItem
            {
                Value = x,
                Text = x,
                Selected = x == model.Product
            })
            .ToList();

        if (!string.IsNullOrWhiteSpace(model.Product) && model.Amount > 0)
        {
            var calc = GetProductCalculation(model.Product, model.Amount, model.IsUkContract);
            model.CommissionPercent = calc.CommissionPercent;
            model.Divider = calc.Divider;
            model.CalculatedCommission = calc.Commission;
            model.CalculatedSu = calc.Su;
        }

        return model;
    }

    private static (decimal CommissionPercent, decimal Divider, decimal Commission, decimal Su) GetProductCalculation(
        string product,
        decimal amount,
        bool isUkContract)
    {
        var rules = GetProductRules();

        if (!rules.ContainsKey(product))
        {
            return (0m, 100000m, 0m, 0m);
        }

        var rule = rules[product];
        var su = rule.Divider <= 0 ? 0m : Math.Round(amount / rule.Divider, 4);

        var ukOnlyProducts = GetUkQuestionProducts();

        decimal commission = Math.Round(amount * (rule.Percent / 100m), 0);

        if (ukOnlyProducts.Contains(product) && isUkContract)
        {
            commission = 0m;
        }

        return (rule.Percent, rule.Divider, commission, su);
    }

    private static HashSet<string> GetUkQuestionProducts()
    {
        return new HashSet<string>
        {
            "Vienna Yes alapdíj, ha a teljes díj  120e Ft-ig /állománydíjas/",
            "Vienna Yes alapdíj, ha a teljes díj 120-145e Ft között /állománydíjas/",
            "Vienna Yes alapdíj, ha a teljes díj 145e Ft-tól /állománydíjas/"
        };
    }

    private static Dictionary<string, (decimal Percent, decimal Divider)> GetProductRules()
    {
        return new Dictionary<string, (decimal Percent, decimal Divider)>
        {
            ["Vienna Plan, Age alapdíj, ha a teljes díj 300 e ft alatti fé.,éves"] = (35m, 175000m),
            ["Vienna Plan, Age alapdíj, ha a teljes díj 300 e ft alatti né."] = (35m, 200000m),
            ["Vienna Plan, Age alapdíj, ha a teljes díj 300 e ft alatti havi CSOB"] = (35m, 270000m),
            ["Vienna Plan, Age alapdíj, ha a teljes díj 300 e ft alatti havi átutalás, kártya"] = (35m, 480000m),

            ["Vienna Plan, Age alapdíj, ha a teljes díj 300 - 310 e ft között fé.,éves"] = (35m, 145000m),
            ["Vienna Plan, Age alapdíj, ha a teljes díj 300 - 310 e ft között ft alatti né."] = (35m, 170000m),
            ["Vienna Plan, Age alapdíj, ha a teljes díj 300 - 310 e ft között havi CSOB"] = (35m, 220000m),
            ["Vienna Plan, Age alapdíj, ha a teljes díj 300 - 310 e ft között havi átutalás, kártya"] = (35m, 400000m),

            ["Vienna Plan, Age alapdíj, ha a teljes díj 310 -410 e ft között  fé.,éves"] = (40m, 145000m),
            ["Vienna Plan, Age alapdíj, ha a teljes díj 310 -410 e ft között né."] = (40m, 170000m),
            ["Vienna Plan, Age alapdíj, ha a teljes díj 310 -410 e ft között havi CSOB"] = (40m, 220000m),
            ["Vienna Plan, Age alapdíj, ha a teljes díj 310 -410 e ft között havi átutalás, kártya"] = (40m, 400000m),

            ["Vienna Plan, Age alapdíj, ha a teljes díj minimum  410 e ft fé.,éves"] = (45m, 145000m),
            ["Vienna Plan, Age alapdíj, ha a teljes díj minimum  410 e ft né."] = (45m, 170000m),
            ["Vienna Plan, Age alapdíj, ha a teljes díj minimum  410 e ft havi CSOB"] = (45m, 220000m),
            ["Vienna Plan, Age alapdíj, ha a teljes díj minimum  410 e ft havi átutalás, kártya"] = (45m, 400000m),

            ["Vienna Plan, Age ÜK"] = (0m, 720000m),
            ["Életbiztosítási  (UL, YES) kiegészítők"] = (75m, 45000m),
            ["Lakásbiztosítás vagy MFO"] = (9m, 27500m),
            ["Vagyon (Business Class, BC; KKV Felelősség/"] = (16.7m, 80000m),
            ["KKV Egészségügyi, Rendezvényszervezői Felelősség"] = (16.7m, 50000m),
            ["KKV Elber, Gépbiztosítás"] = (9.3m, 80000m),
            ["Egyedi vagyon"] = (9.3m, 150000m),
            ["Balesetbiztosítás - Menta"] = (25m, 50000m),

            ["Vienna Yes alapdíj, ha a teljes díj  120e Ft-ig /állománydíjas/"] = (55m, 60000m),
            ["Vienna Yes alapdíj, ha a teljes díj 120-145e Ft között /állománydíjas/"] = (62m, 60000m),
            ["Vienna Yes alapdíj, ha a teljes díj 145e Ft-tól /állománydíjas/"] = (70m, 60000m),

            ["Napnyugta /5év, állománydíj/"] = (70m, 45000m),
            ["Kompakt csoportos élet és baleset"] = (14m, 50000m),
            ["Private-Med Next"] = (12.5m, 200000m),
            ["CASCO"] = (9m, 100000m),
            ["KGFB"] = (2.65m, 350000m),
            ["Utas"] = (0m, 120000m),
            ["Útitárs"] = (0m, 50000m),
            ["Eseti díj"] = (2.5m, 1000000m),
            ["Eseti díj Plan, Age"] = (1.12m, 1000000m),
            ["Eseti díj ÜK"] = (0m, 1000000m)
        };
    }

    private static List<string> GetProducts()
    {
        return GetProductRules().Keys.ToList();
    }
}