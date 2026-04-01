using AdvisorDashboardApp.Data;
using AdvisorDashboardApp.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://*:{port}");
}

builder.Services.AddControllersWithViews();

var renderDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var postgresConnection = builder.Configuration.GetConnectionString("PostgresConnection");
var sqliteConnection = builder.Configuration.GetConnectionString("DefaultConnection");

string? finalConnectionString = null;

if (!string.IsNullOrWhiteSpace(renderDatabaseUrl))
{
    finalConnectionString = BuildRenderPostgresConnectionString(renderDatabaseUrl);
}
else if (!string.IsNullOrWhiteSpace(postgresConnection))
{
    finalConnectionString = postgresConnection;
}
else
{
    finalConnectionString = sqliteConnection;
}

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

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