using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AdvisorDashboardApp.Models;

public class DataEntryViewModel
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

    public List<SelectListItem> Advisors { get; set; } = new();
    public List<SelectListItem> Years { get; set; } = new();
    public List<SelectListItem> Months { get; set; } = new();
    public List<SelectListItem> Products { get; set; } = new();
}