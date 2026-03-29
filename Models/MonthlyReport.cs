using System.ComponentModel.DataAnnotations;

namespace AdvisorDashboardApp.Models;

public class MonthlyReport
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A tanácsadó kiválasztása kötelező.")]
    [Display(Name = "Tanácsadó")]
    public int AdvisorId { get; set; }

    [Required(ErrorMessage = "Az év kiválasztása kötelező.")]
    [Range(2020, 2100, ErrorMessage = "Adj meg érvényes évet.")]
    [Display(Name = "Év")]
    public int Year { get; set; }

    [Required(ErrorMessage = "A hónap kiválasztása kötelező.")]
    [Range(1, 12, ErrorMessage = "A hónap 1 és 12 között lehet.")]
    [Display(Name = "Hónap")]
    public int Month { get; set; }

    [Required(ErrorMessage = "A termék kiválasztása kötelező.")]
    [Display(Name = "Termék")]
    public string Product { get; set; } = string.Empty;

    [Required(ErrorMessage = "Az állománydíj megadása kötelező.")]
    [Range(0, 999999999, ErrorMessage = "Adj meg érvényes állománydíjat.")]
    [Display(Name = "Állománydíj")]
    public decimal Amount { get; set; }

    [Display(Name = "Jutalék")]
    public decimal Commission { get; set; }

    [Display(Name = "SU")]
    public decimal Su { get; set; }

    public string? Notes { get; set; }

    public Advisor? Advisor { get; set; }
}