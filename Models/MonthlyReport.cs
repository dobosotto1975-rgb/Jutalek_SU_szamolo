using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdvisorDashboardApp.Models;

public class MonthlyReport
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A tanácsadó kiválasztása kötelező.")]
    public int AdvisorId { get; set; }

    [ForeignKey(nameof(AdvisorId))]
    public Advisor? Advisor { get; set; }

    [Range(2020, 2100, ErrorMessage = "Adj meg egy érvényes évet.")]
    public int Year { get; set; }

    [Range(1, 12, ErrorMessage = "A hónap 1 és 12 között lehet.")]
    public int Month { get; set; }

    [Required(ErrorMessage = "A termék kiválasztása kötelező.")]
    [StringLength(200)]
    public string Product { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "999999999")]
    public decimal Amount { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal CommissionPercent { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal Divider { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal Commission { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal Su { get; set; }

    public bool IsUkContract { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ContractStartDate { get; set; }

    public bool IsPremiumPaid { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}