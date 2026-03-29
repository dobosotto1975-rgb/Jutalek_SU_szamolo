using System.ComponentModel.DataAnnotations;

namespace AdvisorDashboardApp.Models;

public class Advisor
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A név kötelező.")]
    [Display(Name = "Név")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Telefonszám")]
    public string? Phone { get; set; }

    [Display(Name = "E-mail")]
    public string? Email { get; set; }

    [Display(Name = "Aktív")]
    public bool IsActive { get; set; } = true;

    public List<MonthlyReport> MonthlyReports { get; set; } = new();
}