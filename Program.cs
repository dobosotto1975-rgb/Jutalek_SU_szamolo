using AdvisorDashboardApp.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var rawConnection = GetRawConnectionString(builder.Configuration);

if (!string.IsNullOrWhiteSpace(rawConnection))
{
    var postgresConnection = NormalizePostgresConnectionString(rawConnection);

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(postgresConnection));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite("Data Source=app.db"));
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
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

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    app = "AdvisorDashboardApp",
    utc = DateTime.UtcNow
}));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static string? GetRawConnectionString(ConfigurationManager configuration)
{
    var fromConnectionStrings = configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(fromConnectionStrings))
        return fromConnectionStrings;

    var fromDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(fromDatabaseUrl))
        return fromDatabaseUrl;

    return null;
}

static string NormalizePostgresConnectionString(string input)
{
    if (!input.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
        !input.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        return input;
    }

    var uri = new Uri(input);
    var userInfoParts = uri.UserInfo.Split(':', 2);

    var username = userInfoParts.Length > 0
        ? Uri.UnescapeDataString(userInfoParts[0])
        : string.Empty;

    var password = userInfoParts.Length > 1
        ? Uri.UnescapeDataString(userInfoParts[1])
        : string.Empty;

    var database = uri.AbsolutePath.Trim('/');

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Username = username,
        Password = password,
        Database = database,
        Pooling = true,
        Timeout = 15,
        CommandTimeout = 30,
        TrustServerCertificate = true
    };

    return builder.ConnectionString;
}