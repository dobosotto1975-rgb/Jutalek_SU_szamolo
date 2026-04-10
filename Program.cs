using AdvisorDashboardApp.Data;
using AdvisorDashboardApp.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("=== APP STARTING ===");

// PORT kezelés
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

// Connection string logika
var renderDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var postgresConnection = builder.Configuration.GetConnectionString("PostgresConnection");

// A SQLite fájlt fixen a projekt/content root mappába tesszük
var sqliteFilePath = Path.Combine(builder.Environment.ContentRootPath, "advisor_dashboard.db");
var sqliteConnection = $"Data Source={sqliteFilePath}";

Console.WriteLine($"ContentRootPath: {builder.Environment.ContentRootPath}");
Console.WriteLine($"SQLite file path: {sqliteFilePath}");

string finalConnectionString;
bool usePostgres;

try
{
    if (!string.IsNullOrWhiteSpace(renderDatabaseUrl))
    {
        Console.WriteLine("Using DATABASE_URL from environment.");
        finalConnectionString = BuildRenderPostgresConnectionString(renderDatabaseUrl);
        usePostgres = true;
    }
    else if (!string.IsNullOrWhiteSpace(postgresConnection))
    {
        Console.WriteLine("Using PostgresConnection from configuration.");
        finalConnectionString = postgresConnection;
        usePostgres = true;
    }
    else
    {
        Console.WriteLine("Using SQLite with absolute file path.");
        finalConnectionString = sqliteConnection;
        usePostgres = false;
    }
}
catch (Exception ex)
{
    Console.WriteLine("!!! CONNECTION STRING ERROR !!!");
    Console.WriteLine(ex);
    throw;
}

// DB konfiguráció
if (usePostgres)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(finalConnectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(finalConnectionString));
}

builder.Services.AddScoped<IProductCalculationService, ProductCalculationService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

Console.WriteLine("=== BUILD OK ===");

app.UseForwardedHeaders();

// MIGRATION csak helyi SQLite fejlesztéshez
if (!usePostgres)
{
    try
    {
        using var scope = app.Services.CreateScope();
        Console.WriteLine("=== DB MIGRATION START ===");
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Console.WriteLine("=== DB MIGRATION OK ===");
    }
    catch (Exception ex)
    {
        Console.WriteLine("!!! MIGRATION ERROR (APP STILL RUNS) !!!");
        Console.WriteLine(ex);
    }
}
else
{
    Console.WriteLine("=== POSTGRES MODE: automatic migration skipped ===");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Renderen nincs szükség kötelező https redirectre reverse proxy mögött
if (!usePostgres)
{
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

static string BuildRenderPostgresConnectionString(string databaseUrl)
{
    var uri = new Uri(databaseUrl);

    var userInfoParts = uri.UserInfo.Split(':', 2);
    var username = userInfoParts.Length > 0 ? Uri.UnescapeDataString(userInfoParts[0]) : "";
    var password = userInfoParts.Length > 1 ? Uri.UnescapeDataString(userInfoParts[1]) : "";

    var database = uri.AbsolutePath.TrimStart('/');
    var dbPort = uri.Port > 0 ? uri.Port : 5432;

    return $"Host={uri.Host};Port={dbPort};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}