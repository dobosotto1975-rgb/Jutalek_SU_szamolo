using AdvisorDashboardApp.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AdvisorDashboardApp.Models;

public class DataEntryViewModel : IValidatableObject
{
    [Required(ErrorMessage = "A tanácsadó kiválasztása kötelező.")]
    public int AdvisorId { get; set; }

    [Required(ErrorMessage = "Az év kiválasztása kötelező.")]
    public int Year { get; set; }

    [Required(ErrorMessage = "A hónap kiválasztása kötelező.")]
    public int Month { get; set; }

    [Required(ErrorMessage = "A termék kiválasztása kötelező.")]
    public string Product { get; set; } = string.Empty;

    [Required(ErrorMessage = "Az állománydíj megadása kötelező.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Az állománydíj legyen nagyobb mint 0.")]
    public decimal Amount { get; set; }

    public bool IsUkContract { get; set; }

    public decimal CommissionPercent { get; set; }
    public decimal Divider { get; set; }
    public decimal CalculatedCommission { get; set; }
    public decimal CalculatedSu { get; set; }

    public decimal? YesFullBaseAmount { get; set; }
    public decimal? YesFullTotalAmount { get; set; }

    [Range(0, 100, ErrorMessage = "A kedvezmény % 0 és 100 között lehet.")]
    public decimal? YesDiscountPercent { get; set; }

    public decimal YesFullSupplementAmount { get; set; }
    public decimal YesDiscountedBaseAmount { get; set; }
    public decimal YesDiscountedSupplementAmount { get; set; }
    public decimal YesDiscountedTotalAmount { get; set; }

    public List<SelectListItem> Advisors { get; set; } = new();
    public List<SelectListItem> Years { get; set; } = new();
    public List<SelectListItem> Months { get; set; } = new();
    public List<SelectListItem> Products { get; set; } = new();

    public bool RequiresYesDetails()
    {
        return ProductCatalog.RequiresUkQuestionProduct(Product);
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!RequiresYesDetails())
            yield break;

        if (!YesFullBaseAmount.HasValue)
        {
            yield return new ValidationResult(
                "A teljes alapdíj megadása kötelező ennél a terméknél.",
                new[] { nameof(YesFullBaseAmount) });
        }

        if (!YesFullTotalAmount.HasValue)
        {
            yield return new ValidationResult(
                "A teljes összes díj megadása kötelező ennél a terméknél.",
                new[] { nameof(YesFullTotalAmount) });
        }

        if (!YesDiscountPercent.HasValue)
        {
            yield return new ValidationResult(
                "A kedvezmény % megadása kötelező ennél a terméknél.",
                new[] { nameof(YesDiscountPercent) });
        }

        if (YesFullBaseAmount.HasValue && YesFullBaseAmount.Value < 0)
        {
            yield return new ValidationResult(
                "A teljes alapdíj nem lehet negatív.",
                new[] { nameof(YesFullBaseAmount) });
        }

        if (YesFullTotalAmount.HasValue && YesFullTotalAmount.Value < 0)
        {
            yield return new ValidationResult(
                "A teljes összes díj nem lehet negatív.",
                new[] { nameof(YesFullTotalAmount) });
        }

        if (YesFullBaseAmount.HasValue &&
            YesFullTotalAmount.HasValue &&
            YesFullTotalAmount.Value < YesFullBaseAmount.Value)
        {
            yield return new ValidationResult(
                "A teljes összes díj nem lehet kisebb, mint a teljes alapdíj.",
                new[] { nameof(YesFullTotalAmount), nameof(YesFullBaseAmount) });
        }
    }
}