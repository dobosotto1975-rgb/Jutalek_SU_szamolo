using Microsoft.AspNetCore.Mvc.Rendering;

namespace AdvisorDashboardApp.Models;

public class MonthlySummaryViewModel
{
    public List<MonthlySummaryRow> Rows { get; set; } = new();
    public decimal GrandTotalAmount { get; set; }
    public decimal GrandTotalCommission { get; set; }
}

public class MonthlySummaryRow
{
    public string AdvisorName { get; set; } = string.Empty;
    public int ReportCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalCommission { get; set; }
}

public class AdvisorMonthlySummaryFilterViewModel
{
    public int SelectedYear { get; set; }
    public int SelectedMonth { get; set; }
    public int? SelectedAdvisorId { get; set; }

    public List<SelectListItem> Years { get; set; } = new();
    public List<SelectListItem> Months { get; set; } = new();
    public List<SelectListItem> Advisors { get; set; } = new();

    public List<AdvisorMonthlySummaryRow> Rows { get; set; } = new();

    public int ReportCount { get; set; }
    public decimal TotalMonthlyAmount { get; set; }
    public decimal TotalCumulativeAmount { get; set; }
    public decimal TotalMonthlySu { get; set; }
    public decimal TotalBaseCommission { get; set; }
    public decimal TotalBonusAmount { get; set; }
    public decimal TotalFinalCommission { get; set; }
}

public class AdvisorMonthlySummaryRow
{
    public int AdvisorId { get; set; }
    public string AdvisorName { get; set; } = string.Empty;

    public int ReportCount { get; set; }

    public decimal MonthlyAmount { get; set; }
    public decimal CumulativeAmount { get; set; }
    public decimal MonthlySu { get; set; }

    public decimal BaseCommission { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal FinalCommission { get; set; }

    public decimal BonusPercent
    {
        get
        {
            if (MonthlySu >= 9m) return 45m;
            if (MonthlySu >= 4.5m) return 20m;
            return 0m;
        }
    }

    public string BonusLabel
    {
        get
        {
            if (MonthlySu >= 9m) return "+45%";
            if (MonthlySu >= 4.5m) return "+20%";
            return "0%";
        }
    }
}