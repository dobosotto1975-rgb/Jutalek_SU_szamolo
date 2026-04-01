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

if (!string.IsNullOrWhiteSpace(renderDatabaseUrl))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(renderDatabaseUrl));
}
else if (!string.IsNullOrWhiteSpace(postgresConnection))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(postgresConnection));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(sqliteConnection));
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