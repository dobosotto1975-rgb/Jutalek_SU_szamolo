using System.Text.Json;

namespace AdvisorDashboardApp.Services;

public static class ProductCatalog
{
    public sealed record ProductDefinition(
        string Code,
        string Label,
        decimal Divider,
        decimal Percent,
        string Mode,
        bool RequiresUkQuestion,
        params string[] Aliases);

    private static readonly IReadOnlyList<ProductDefinition> Definitions = new List<ProductDefinition>
    {
        new(
            "VIENNA_PLAN_AGE_LT300_FE",
            "Vienna Plan, Age alapdíj, ha a teljes díj 300 e ft alatti fé.,éves",
            175000m, 35m, "percent", false,
            "Vienna Plan, Age alapdíj, ha a teljes díj 300 e ft alatti fé., éves"
        ),
        new(
            "VIENNA_PLAN_AGE_LT300_NE",
            "Vienna Plan, Age alapdíj, ha a teljes díj 300 e ft alatti né.",
            200000m, 35m, "percent", false
        ),
        new(
            "VIENNA_PLAN_AGE_LT300_CSOB",
            "Vienna Plan, Age alapdíj, ha a teljes díj 300 e ft alatti havi CSOB",
            270000m, 35m, "percent", false
        ),
        new(
            "VIENNA_PLAN_AGE_LT300_TRANSFER",
            "Vienna Plan, Age alapdíj, ha a teljes díj 300 e ft alatti havi átutalás, kártya",
            480000m, 35m, "percent", false
        ),

        new(
            "VIENNA_PLAN_AGE_300_310_FE",
            "Vienna Plan, Age alapdíj, ha a teljes díj 300 - 310 e ft között fé.,éves",
            145000m, 35m, "percent", false,
            "Vienna Plan, Age alapdíj, ha a teljes díj 300-310 e ft között fé.,éves",
            "Vienna Plan, Age alapdíj, ha a teljes díj 300 - 310 e ft között fé., éves"
        ),
        new(
            "VIENNA_PLAN_AGE_300_310_NE",
            "Vienna Plan, Age alapdíj, ha a teljes díj 300 - 310 e ft között né.",
            170000m, 35m, "percent", false,
            "Vienna Plan, Age alapdíj, ha a teljes díj 300 - 310 e ft között ft alatti né.",
            "Vienna Plan, Age alapdíj, ha a teljes díj 300-310 e ft között né."
        ),
        new(
            "VIENNA_PLAN_AGE_300_310_CSOB",
            "Vienna Plan, Age alapdíj, ha a teljes díj 300 - 310 e ft között havi CSOB",
            220000m, 35m, "percent", false,
            "Vienna Plan, Age alapdíj, ha a teljes díj 300-310 e ft között havi CSOB"
        ),
        new(
            "VIENNA_PLAN_AGE_300_310_TRANSFER",
            "Vienna Plan, Age alapdíj, ha a teljes díj 300 - 310 e ft között havi átutalás, kártya",
            400000m, 35m, "percent", false,
            "Vienna Plan, Age alapdíj, ha a teljes díj 300-310 e ft között havi átutalás, kártya"
        ),

        new(
            "VIENNA_PLAN_AGE_310_410_FE",
            "Vienna Plan, Age alapdíj, ha a teljes díj 310 -410 e ft között  fé.,éves",
            145000m, 40m, "percent", false,
            "Vienna Plan, Age alapdíj, ha a teljes díj 310 - 410 e ft között fé.,éves",
            "Vienna Plan, Age alapdíj, ha a teljes díj 310-410 e ft között fé.,éves"
        ),
        new(
            "VIENNA_PLAN_AGE_310_410_NE",
            "Vienna Plan, Age alapdíj, ha a teljes díj 310 -410 e ft között né.",
            170000m, 40m, "percent", false,
            "Vienna Plan, Age alapdíj, ha a teljes díj 310 - 410 e ft között né.",
            "Vienna Plan, Age alapdíj, ha a teljes díj 310-410 e ft között né."
        ),
        new(
            "VIENNA_PLAN_AGE_310_410_CSOB",
            "Vienna Plan, Age alapdíj, ha a teljes díj 310 -410 e ft között havi CSOB",
            220000m, 40m, "percent", false,
            "Vienna Plan, Age alapdíj, ha a teljes díj 310 - 410 e ft között havi CSOB",
            "Vienna Plan, Age alapdíj, ha a teljes díj 310-410 e ft között havi CSOB"
        ),
        new(
            "VIENNA_PLAN_AGE_310_410_TRANSFER",
            "Vienna Plan, Age alapdíj, ha a teljes díj 310 -410 e ft között havi átutalás, kártya",
            400000m, 40m, "percent", false,
            "Vienna Plan, Age alapdíj, ha a teljes díj 310 - 410 e ft között havi átutalás, kártya",
            "Vienna Plan, Age alapdíj, ha a teljes díj 310-410 e ft között havi átutalás, kártya"
        ),

        new(
            "VIENNA_PLAN_AGE_GTE410_FE",
            "Vienna Plan, Age alapdíj, ha a teljes díj minimum  410 e ft fé.,éves",
            145000m, 45m, "percent", false,
            "Vienna Plan, Age alapdíj, ha a teljes díj minimum 410 e ft fé.,éves",
            "Vienna Plan, Age alapdíj, ha a teljes díj minimum 410 e ft fé., éves"
        ),
        new(
            "VIENNA_PLAN_AGE_GTE410_NE",
            "Vienna Plan, Age alapdíj, ha a teljes díj minimum  410 e ft né.",
            170000m, 45m, "percent", false,
            "Vienna Plan, Age alapdíj, ha a teljes díj minimum 410 e ft né."
        ),
        new(
            "VIENNA_PLAN_AGE_GTE410_CSOB",
            "Vienna Plan, Age alapdíj, ha a teljes díj minimum  410 e ft havi CSOB",
            220000m, 45m, "percent", false,
            "Vienna Plan, Age alapdíj, ha a teljes díj minimum 410 e ft havi CSOB"
        ),
        new(
            "VIENNA_PLAN_AGE_GTE410_TRANSFER",
            "Vienna Plan, Age alapdíj, ha a teljes díj minimum  410 e ft havi átutalás, kártya",
            400000m, 45m, "percent", false,
            "Vienna Plan, Age alapdíj, ha a teljes díj minimum 410 e ft havi átutalás, kártya"
        ),

        new(
            "VIENNA_PLAN_AGE_UK",
            "Vienna Plan, Age ÜK",
            720000m, 0m, "percent", false
        ),
        new(
            "UL_YES_KIEG",
            "Életbiztosítási  (UL, YES) kiegészítők",
            45000m, 75m, "percent", false,
            "Életbiztosítási (UL, YES) kiegészítők"
        ),
        new(
            "LAKAS_MFO",
            "Lakásbiztosítás vagy MFO",
            27500m, 9m, "percent", false
        ),
        new(
            "VAGYON_BC_KKV",
            "Vagyon (Business Class, BC; KKV Felelősség/",
            80000m, 16.7m, "percent", false
        ),
        new(
            "KKV_EGESZSEG_RENDEZVENY",
            "KKV Egészségügyi, Rendezvényszervezői Felelősség",
            50000m, 16.7m, "percent", false
        ),
        new(
            "KKV_ELBER_GEP",
            "KKV Elber, Gépbiztosítás",
            80000m, 9.3m, "percent", false
        ),
        new(
            "EGYEDI_VAGYON",
            "Egyedi vagyon",
            150000m, 9.3m, "percent", false
        ),
        new(
            "MENTA",
            "Balesetbiztosítás - Menta",
            50000m, 25m, "percent", false
        ),

        new(
            "VIENNA_YES_LT120",
            "Vienna Yes alapdíj, ha a teljes díj  120e Ft-ig /állománydíjas/",
            60000m, 55m, "percent", true,
            "Vienna Yes alapdíj, ha a teljes díj 120e Ft-ig /állománydíjas/"
        ),
        new(
            "VIENNA_YES_120_145",
            "Vienna Yes alapdíj, ha a teljes díj 120-145e Ft között /állománydíjas/",
            60000m, 62m, "percent", true,
            "Vienna Yes alapdíj, ha a teljes díj 120 - 145e Ft között /állománydíjas/"
        ),
        new(
            "VIENNA_YES_GTE145",
            "Vienna Yes alapdíj, ha a teljes díj 145e Ft-tól /állománydíjas/",
            60000m, 70m, "percent", true
        ),

        new(
            "NAPNYUGTA",
            "Napnyugta /5év, állománydíj/",
            45000m, 70m, "percent", false
        ),
        new(
            "KOMPAKT",
            "Kompakt csoportos élet és baleset",
            50000m, 14m, "percent", false
        ),
        new(
            "PRIVATE_MED_NEXT",
            "Private-Med Next",
            200000m, 12.5m, "percent", false
        ),
        new(
            "CASCO",
            "CASCO",
            100000m, 9m, "percent", false
        ),
        new(
            "KGFB",
            "KGFB",
            350000m, 2.65m, "percent", false
        ),

        new(
            "UTAS",
            "Utas",
            120000m, 0m, "none", false
        ),
        new(
            "UTITARS",
            "Útitárs",
            50000m, 0m, "none", false,
            "Utitárs"
        ),

        new(
            "ESETI_DIJ",
            "Eseti díj",
            1000000m, 2.5m, "percent", false
        ),
        new(
            "ESETI_DIJ_PLAN_AGE",
            "Eseti díj Plan, Age",
            1000000m, 1.12m, "percent", false
        ),
        new(
            "ESETI_DIJ_UK",
            "Eseti díj ÜK",
            1000000m, 0m, "percent", false
        )
    };

    private static readonly IReadOnlyDictionary<string, ProductDefinition> ByCode =
        Definitions.ToDictionary(x => x.Code, x => x, StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyDictionary<string, string> AliasToCode = BuildAliasMap();

    public static IReadOnlyList<string> GetDisplayProducts()
    {
        return Definitions.Select(x => x.Label).ToList();
    }

    public static ProductDefinition? ResolveDefinition(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var trimmed = input.Trim();

        if (ByCode.TryGetValue(trimmed, out var byCode))
            return byCode;

        var normalized = Normalize(trimmed);

        if (AliasToCode.TryGetValue(normalized, out var code) && ByCode.TryGetValue(code, out var byAlias))
            return byAlias;

        return null;
    }

    public static bool RequiresUkQuestionProduct(string? input)
    {
        return ResolveDefinition(input)?.RequiresUkQuestion ?? false;
    }

    public static string GetDisplayLabel(string? input)
    {
        return ResolveDefinition(input)?.Label ?? (input ?? string.Empty).Trim();
    }

    public static string GetRulesJsonForClient()
    {
        var dict = new Dictionary<string, object>(StringComparer.Ordinal);

        foreach (var definition in Definitions)
        {
            var payload = new
            {
                divisor = definition.Divider,
                percent = definition.Percent / 100m,
                mode = definition.Mode,
                requiresUkQuestion = definition.RequiresUkQuestion
            };

            dict[definition.Label] = payload;
            dict[definition.Code] = payload;

            foreach (var alias in definition.Aliases)
            {
                dict[alias] = payload;
            }
        }

        return JsonSerializer.Serialize(dict);
    }

    private static IReadOnlyDictionary<string, string> BuildAliasMap()
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var definition in Definitions)
        {
            AddAlias(map, definition.Code, definition.Code);
            AddAlias(map, definition.Label, definition.Code);

            foreach (var alias in definition.Aliases)
            {
                AddAlias(map, alias, definition.Code);
            }
        }

        return map;
    }

    private static void AddAlias(IDictionary<string, string> map, string source, string code)
    {
        var normalized = Normalize(source);
        if (!map.ContainsKey(normalized))
        {
            map[normalized] = code;
        }
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var text = value.Trim().ToLowerInvariant();

        text = text.Replace('–', '-');
        text = text.Replace('—', '-');
        text = text.Replace("  ", " ");

        while (text.Contains("  "))
        {
            text = text.Replace("  ", " ");
        }

        text = text.Replace(" ,", ",");
        text = text.Replace(" .", ".");
        text = text.Replace(" /", "/");
        text = text.Replace("/ ", "/");

        return text;
    }
}