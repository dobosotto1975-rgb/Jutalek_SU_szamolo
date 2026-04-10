using AdvisorDashboardApp.Data;
using AdvisorDashboardApp.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("=== APP STARTING ===");

// PORT kezelés (Render)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    Console.WriteLine($"PORT detected: {port}");
    builder.WebHost.UseUrls($"http://*:{port}");
}
else
{
    Console.WriteLine("PORT not found, fallback to 10000");
    builder.WebHost.UseUrls("http://*:10000");
}

builder.Services.AddControllersWithViews();

// Connection string logika
var renderDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var postgresConnection = builder.Configuration.GetConnectionString("PostgresConnection");
var sqliteConnection = builder.Configuration.GetConnectionString("DefaultConnection");

string? finalConnectionString = null;

try
{
    if (!string.IsNullOrWhiteSpace(renderDatabaseUrl))
    {
        Console.WriteLine("Using DATABASE_URL from Render");
        finalConnectionString = BuildRenderPostgresConnectionString(renderDatabaseUrl);
    }
    else if (!string.IsNullOrWhiteSpace(postgresConnection))
    {
        Console.WriteLine("Using PostgresConnection from appsettings");
        finalConnectionString = postgresConnection;
    }
    else
    {
        Console.WriteLine("Using SQLite fallback");
        finalConnectionString = sqliteConnection;
    }
}
catch (Exception ex)
{
    Console.WriteLine("!!! CONNECTION STRING ERROR !!!");
    Console.WriteLine(ex.ToString());
    throw;
}

if (string.IsNullOrWhiteSpace(finalConnectionString))
{
    throw new Exception("No valid connection string was found.");
}

// DB konfiguráció
if (!string.IsNullOrWhiteSpace(renderDatabaseUrl) || !string.IsNullOrWhiteSpace(postgresConnection))
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

var app = builder.Build();

Console.WriteLine("=== BUILD OK ===");

// SAFE MIGRATION
try
{
    using (var scope = app.Services.CreateScope())
    {
        Console.WriteLine("=== DB MIGRATION START ===");
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Console.WriteLine("=== DB MIGRATION OK ===");
    }
}
catch (Exception ex)
{
    Console.WriteLine("!!! MIGRATION ERROR (APP STILL RUNS) !!!");
    Console.WriteLine(ex.ToString());
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// Health / ping endpointok
app.MapGet("/health", () => Results.Text("OK", "text/plain"));
app.MapGet("/ping", () => Results.Text(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "text/plain"));

// MVC route
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