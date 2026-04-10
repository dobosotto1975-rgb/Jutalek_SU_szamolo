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
    public decimal TotalCommission { get; set; }
    public decimal CurrentMonthAmount { get; set; }

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
            if (MonthlySu <= 0m) return 0m;

            var percent = (MonthlySu / 9m) * 100m;

            if (percent < 0m) return 0m;
            if (percent > 100m) return 100m;

            return Math.Round(percent, 2);
        }
    }

    public string SuStatusText
    {
        get
        {
            if (MonthlySu >= 9m) return "Kiemelt bónusz sáv";
            if (MonthlySu >= 4.5m) return "Bónusz sáv";
            if (MonthlySu > 0m) return "Még nincs bónusz sáv";
            return "Nincs termelés";
        }
    }

    public string GaugeColor
    {
        get
        {
            if (MonthlySu >= 9m) return "#22c55e";
            if (MonthlySu >= 4.5m) return "#facc15";
            if (MonthlySu > 0m) return "#60a5fa";
            return "#94a3b8";
        }
    }
}