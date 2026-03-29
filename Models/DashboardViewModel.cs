using Microsoft.AspNetCore.Mvc.Rendering;

namespace AdvisorDashboardApp.Models;

public class DashboardViewModel
{
    public int SelectedYear { get; set; }
    public int SelectedMonth { get; set; }

    public List<SelectListItem> Years { get; set; } = new();
    public List<SelectListItem> Months { get; set; } = new();

    public int AdvisorCount { get; set; }
    public int MonthlyReportCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal CurrentMonthAmount { get; set; }
    public decimal TotalCommission { get; set; }

    public List<MonthlyReport> LatestReports { get; set; } = new();
    public List<AdvisorDashboardRow> AdvisorRows { get; set; } = new();
}

public class AdvisorDashboardRow
{
    public int AdvisorId { get; set; }
    public string AdvisorName { get; set; } = string.Empty;

    public decimal MonthlyAmount { get; set; }
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

    public decimal ProgressPercent
    {
        get
        {
            var percent = MonthlySu <= 0 ? 0 : (MonthlySu / 20m) * 100m;
            if (percent < 0) return 0;
            if (percent > 100) return 100;
            return percent;
        }
    }

    public string GaugeColor
    {
        get
        {
            if (MonthlySu >= 9m) return "#22c55e";
            if (MonthlySu >= 4.5m) return "#f59e0b";
            return "#6b7280";
        }
    }

    public string SuStatusText
    {
        get
        {
            if (MonthlySu >= 9m) return "Bónusz sáv: +45%";
            if (MonthlySu >= 4.5m) return "Bónusz sáv: +20%";
            return "Bónusz sáv: 0%";
        }
    }
}