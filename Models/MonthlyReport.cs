using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdvisorDashboardApp.Models;

public class MonthlyReport
{
    public int Id { get; set; }

    [Required]
    public int AdvisorId { get; set; }
    public Advisor? Advisor { get; set; }

    [Required]
    public int Year { get; set; }

    [Required]
    [Range(1, 12)]
    public int Month { get; set; }

    [Required]
    [StringLength(300)]
    public string Product { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CommissionPercent { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Divider { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Commission { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Su { get; set; }

    public bool IsUkContract { get; set; } = false;

    [StringLength(500)]
    public string? Notes { get; set; }
}