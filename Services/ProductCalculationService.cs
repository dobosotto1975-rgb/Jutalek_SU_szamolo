namespace AdvisorDashboardApp.Services;

public interface IProductCalculationService
{
    IReadOnlyList<string> GetProducts();
    bool RequiresUkQuestion(string? product);
    ProductCalculationResult Calculate(string? product, decimal amount, bool isUkContract);
    string GetRulesJsonForClient();
}

public sealed class ProductCalculationService : IProductCalculationService
{
    public IReadOnlyList<string> GetProducts()
    {
        return ProductCatalog.GetDisplayProducts();
    }

    public bool RequiresUkQuestion(string? product)
    {
        return ProductCatalog.RequiresUkQuestionProduct(product);
    }

    public ProductCalculationResult Calculate(string? product, decimal amount, bool isUkContract)
    {
        var definition = ProductCatalog.ResolveDefinition(product);

        if (definition is null)
            return ProductCalculationResult.Empty();

        var su = definition.Divider > 0
            ? Math.Round(amount / definition.Divider, 4)
            : 0m;

        var commission = 0m;

        if (string.Equals(definition.Mode, "percent", StringComparison.OrdinalIgnoreCase))
        {
            commission = Math.Round(amount * (definition.Percent / 100m), 0);
        }

        if (definition.RequiresUkQuestion && isUkContract)
        {
            commission = 0m;
        }

        return new ProductCalculationResult
        {
            Product = definition.Label,
            CommissionPercent = definition.Percent,
            Divider = definition.Divider,
            Commission = commission,
            Su = su,
            RequiresUkQuestion = definition.RequiresUkQuestion,
            IsUkContract = definition.RequiresUkQuestion && isUkContract
        };
    }

    public string GetRulesJsonForClient()
    {
        return ProductCatalog.GetRulesJsonForClient();
    }
}

public sealed class ProductCalculationResult
{
    public string Product { get; set; } = string.Empty;
    public decimal CommissionPercent { get; set; }
    public decimal Divider { get; set; }
    public decimal Commission { get; set; }
    public decimal Su { get; set; }
    public bool RequiresUkQuestion { get; set; }
    public bool IsUkContract { get; set; }

    public static ProductCalculationResult Empty()
    {
        return new ProductCalculationResult
        {
            Product = string.Empty,
            CommissionPercent = 0m,
            Divider = 0m,
            Commission = 0m,
            Su = 0m,
            RequiresUkQuestion = false,
            IsUkContract = false
        };
    }
}