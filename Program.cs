using AdvisorDashboardApp.Data;
using AdvisorDashboardApp.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("=== APP STARTING ===");

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    Console.WriteLine($"PORT detected: {port}");
    builder.WebHost.UseUrls($"http://*:{port}");
}
else
{
    Console.WriteLine("PORT not found, using local defaults from launchSettings or ASP.NET.");
}

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IProductCalculationService, ProductCalculationService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var connectionInfo = ResolveDatabaseConfiguration(builder.Configuration, builder.Environment);

Console.WriteLine($"ContentRootPath: {builder.Environment.ContentRootPath}");
Console.WriteLine($"Database provider: {(connectionInfo.UsePostgres ? "PostgreSQL" : "SQLite")}");
Console.WriteLine($"Connection source: {connectionInfo.Source}");

if (connectionInfo.UsePostgres)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionInfo.ConnectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionInfo.ConnectionString));
}

var app = builder.Build();

Console.WriteLine("=== BUILD OK ===");

app.UseForwardedHeaders();

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    Console.WriteLine("=== DB MIGRATION START ===");
    db.Database.Migrate();
    Console.WriteLine("=== DB MIGRATION OK ===");
}
catch (Exception ex)
{
    Console.WriteLine("!!! DB MIGRATION ERROR !!!");
    Console.WriteLine(ex);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapGet("/health", () => Results.Text("OK", "text/plain"));
app.MapGet("/ping", () => Results.Text(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "text/plain"));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Console.WriteLine("=== APP RUNNING ===");

app.Run();

static DatabaseConnectionInfo ResolveDatabaseConfiguration(IConfiguration configuration, IWebHostEnvironment environment)
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        return new DatabaseConnectionInfo
        {
            UsePostgres = true,
            Source = "DATABASE_URL",
            ConnectionString = BuildPostgresConnectionStringFromUrl(databaseUrl)
        };
    }

    var postgresConnection =
        configuration.GetConnectionString("PostgresConnection") ??
        configuration.GetConnectionString("DefaultConnection");

    if (!string.IsNullOrWhiteSpace(postgresConnection) &&
        LooksLikePostgresConnection(postgresConnection))
    {
        return new DatabaseConnectionInfo
        {
            UsePostgres = true,
            Source = "appsettings PostgresConnection/DefaultConnection",
            ConnectionString = postgresConnection
        };
    }

    var sqliteFilePath = Path.Combine(environment.ContentRootPath, "advisor_dashboard.db");
    var sqliteConnection = $"Data Source={sqliteFilePath}";

    return new DatabaseConnectionInfo
    {
        UsePostgres = false,
        Source = "local SQLite fallback",
        ConnectionString = sqliteConnection
    };
}

static bool LooksLikePostgresConnection(string connectionString)
{
    var text = connectionString.ToLowerInvariant();

    return text.Contains("host=") ||
           text.Contains("server=") ||
           text.Contains("username=") ||
           text.Contains("user id=") ||
           text.Contains("database=");
}

static string BuildPostgresConnectionStringFromUrl(string databaseUrl)
{
    if (databaseUrl.StartsWith("Host=", StringComparison.OrdinalIgnoreCase))
    {
        return databaseUrl;
    }

    var uri = new Uri(databaseUrl);

    var userInfoParts = uri.UserInfo.Split(':', 2);
    var username = userInfoParts.Length > 0 ? Uri.UnescapeDataString(userInfoParts[0]) : string.Empty;
    var password = userInfoParts.Length > 1 ? Uri.UnescapeDataString(userInfoParts[1]) : string.Empty;
    var database = uri.AbsolutePath.TrimStart('/');
    var dbPort = uri.Port > 0 ? uri.Port : 5432;

    return $"Host={uri.Host};Port={dbPort};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}

sealed class DatabaseConnectionInfo
{
    public bool UsePostgres { get; set; }
    public string Source { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}