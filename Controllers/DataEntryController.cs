using AdvisorDashboardApp.Data;
using AdvisorDashboardApp.Models;
using AdvisorDashboardApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdvisorDashboardApp.Controllers;

public class DataEntryController : Controller
{
    private readonly AppDbContext _context;
    private readonly IProductCalculationService _productCalculationService;

    public DataEntryController(
        AppDbContext context,
        IProductCalculationService productCalculationService)
    {
        _context = context;
        _productCalculationService = productCalculationService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await BuildViewModelAsync(new DataEntryViewModel
        {
            Year = DateTime.Now.Year,
            Month = DateTime.Now.Month,
            ContractStartDate = DateTime.Today,
            IsPremiumPaid = true
        });

        LoadRuleJson();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(DataEntryViewModel model)
    {
        ApplyYesCalculations(model);

        var advisorExists = await _context.Advisors
            .AsNoTracking()
            .AnyAsync(x => x.Id == model.AdvisorId && x.IsActive);

        if (!advisorExists)
        {
            ModelState.AddModelError(nameof(model.AdvisorId), "A kiválasztott tanácsadó nem létezik vagy inaktív.");
        }

        var canonicalProduct = ProductCatalog.GetDisplayLabel(model.Product);
        if (string.IsNullOrWhiteSpace(canonicalProduct))
        {
            ModelState.AddModelError(nameof(model.Product), "A kiválasztott termék érvénytelen.");
        }

        if (!ModelState.IsValid)
        {
            model = await BuildViewModelAsync(model);
            LoadRuleJson();
            return View(model);
        }

        var calc = _productCalculationService.Calculate(canonicalProduct, model.Amount, model.IsUkContract);

        var report = new MonthlyReport
        {
            AdvisorId = model.AdvisorId,
            Year = model.Year,
            Month = model.Month,
            Product = canonicalProduct,
            Amount = model.Amount,
            CommissionPercent = calc.CommissionPercent,
            Divider = calc.Divider,
            Commission = calc.Commission,
            Su = calc.Su,
            IsUkContract = calc.IsUkContract,
            ContractStartDate = model.ContractStartDate,
            IsPremiumPaid = model.IsPremiumPaid
        };

        try
        {
            _context.MonthlyReports.Add(report);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Az adat sikeresen rögzítve lett.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            var dbMessage = ex.InnerException?.Message ?? ex.Message;
            ModelState.AddModelError(string.Empty, $"Az adatbázisba mentés nem sikerült: {dbMessage}");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Váratlan hiba történt mentés közben: {ex.Message}");
        }

        model = await BuildViewModelAsync(model);
        LoadRuleJson();
        return View(model);
    }

    private async Task<DataEntryViewModel> BuildViewModelAsync(DataEntryViewModel model)
    {
        var advisors = await _context.Advisors
            .AsNoTracking()
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

        var products = _productCalculationService.GetProducts();
        var selectedProduct = ProductCatalog.GetDisplayLabel(model.Product);

        model.Products = products
            .Select(x => new SelectListItem
            {
                Value = x,
                Text = x,
                Selected = x == selectedProduct
            })
            .ToList();

        model.Product = selectedProduct ?? string.Empty;

        ApplyYesCalculations(model);

        if (!string.IsNullOrWhiteSpace(model.Product) && model.Amount > 0)
        {
            var calc = _productCalculationService.Calculate(model.Product, model.Amount, model.IsUkContract);
            model.CommissionPercent = calc.CommissionPercent;
            model.Divider = calc.Divider;
            model.CalculatedCommission = calc.Commission;
            model.CalculatedSu = calc.Su;
            model.IsUkContract = calc.IsUkContract;
        }
        else
        {
            model.CommissionPercent = 0m;
            model.Divider = 0m;
            model.CalculatedCommission = 0m;
            model.CalculatedSu = 0m;

            if (!_productCalculationService.RequiresUkQuestion(model.Product))
            {
                model.IsUkContract = false;
            }
        }

        return model;
    }

    private static void ApplyYesCalculations(DataEntryViewModel model)
    {
        model.YesFullSupplementAmount = 0m;
        model.YesDiscountedBaseAmount = 0m;
        model.YesDiscountedSupplementAmount = 0m;
        model.YesDiscountedTotalAmount = 0m;

        if (!model.RequiresYesDetails())
            return;

        var fullBase = model.YesFullBaseAmount ?? 0m;
        var fullTotal = model.YesFullTotalAmount ?? 0m;
        var discountPercent = model.YesDiscountPercent ?? 0m;

        if (fullTotal < fullBase)
        {
            return;
        }

        var discountRate = discountPercent / 100m;
        var supplement = fullTotal - fullBase;
        var discountedBase = fullBase * (1m - discountRate);
        var discountedSupplement = supplement * (1m - discountRate);
        var discountedTotal = discountedBase + discountedSupplement;

        model.YesFullSupplementAmount = Math.Round(supplement, 2);
        model.YesDiscountedBaseAmount = Math.Round(discountedBase, 2);
        model.YesDiscountedSupplementAmount = Math.Round(discountedSupplement, 2);
        model.YesDiscountedTotalAmount = Math.Round(discountedTotal, 2);
    }

    private void LoadRuleJson()
    {
        ViewBag.ProductRulesJson = _productCalculationService.GetRulesJsonForClient();
    }
}