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