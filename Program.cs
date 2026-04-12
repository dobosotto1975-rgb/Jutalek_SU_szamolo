using AdvisorDashboardApp.Data;
using AdvisorDashboardApp.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
    Console.WriteLine("PORT not found, using local defaults.");
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

var dbConfig = ResolveDatabaseConfiguration(builder.Configuration, builder.Environment);

Console.WriteLine($"ContentRootPath: {builder.Environment.ContentRootPath}");
Console.WriteLine($"Database provider: {(dbConfig.UsePostgres ? "PostgreSQL" : "SQLite")}");
Console.WriteLine($"Connection source: {dbConfig.Source}");

if (dbConfig.UsePostgres)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(dbConfig.ConnectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(dbConfig.ConnectionString));
}

var app = builder.Build();

Console.WriteLine("=== BUILD OK ===");

app.UseForwardedHeaders();

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    Console.WriteLine("=== DB STARTUP CHECK START ===");

    if (dbConfig.UsePostgres)
    {
        await EnsurePostgresDatabaseAsync(db);
    }
    else
    {
        Console.WriteLine("=== SQLITE MIGRATION START ===");
        await db.Database.MigrateAsync();
        Console.WriteLine("=== SQLITE MIGRATION OK ===");
    }

    Console.WriteLine("=== DB STARTUP CHECK OK ===");
}
catch (Exception ex)
{
    Console.WriteLine("!!! DB STARTUP ERROR !!!");
    Console.WriteLine(ex);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
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
    Console.WriteLine($"SQLite file path: {sqliteFilePath}");

    return new DatabaseConnectionInfo
    {
        UsePostgres = false,
        Source = "local SQLite fallback",
        ConnectionString = $"Data Source={sqliteFilePath}"
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

static async Task EnsurePostgresDatabaseAsync(AppDbContext db)
{
    Console.WriteLine("=== POSTGRES STARTUP CHECK START ===");

    var canConnect = await db.Database.CanConnectAsync();
    if (!canConnect)
    {
        throw new Exception("Nem sikerült csatlakozni a PostgreSQL adatbázishoz.");
    }

    var historyTableExists = await TableExistsAsync(db, "__EFMigrationsHistory");
    var advisorsTableExists = await TableExistsAsync(db, "Advisors");
    var monthlyReportsTableExists = await TableExistsAsync(db, "MonthlyReports");

    Console.WriteLine($"History table exists: {historyTableExists}");
    Console.WriteLine($"Advisors table exists: {advisorsTableExists}");
    Console.WriteLine($"MonthlyReports table exists: {monthlyReportsTableExists}");

    if (!advisorsTableExists && !monthlyReportsTableExists)
    {
        Console.WriteLine("=== CLEAN DATABASE DETECTED -> RUN MIGRATIONS ===");
        await db.Database.MigrateAsync();
        Console.WriteLine("=== MIGRATIONS OK ===");
        return;
    }

    var initialMigrationId = "20260410201753_InitialCreateClean";

    if (historyTableExists)
    {
        var initialRecorded = await MigrationExistsInHistoryAsync(db, initialMigrationId);
        Console.WriteLine($"Initial migration recorded: {initialRecorded}");

        if (!initialRecorded && advisorsTableExists && monthlyReportsTableExists)
        {
            Console.WriteLine("=== EXISTING TABLES FOUND WITHOUT INITIAL HISTORY -> INSERTING HISTORY RECORD ===");

            await db.Database.ExecuteSqlRawAsync($@"
INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
SELECT '{initialMigrationId}', '8.0.8'
WHERE NOT EXISTS (
    SELECT 1
    FROM ""__EFMigrationsHistory""
    WHERE ""MigrationId"" = '{initialMigrationId}'
);");

            Console.WriteLine("=== INITIAL HISTORY RECORD INSERTED ===");
        }
    }

    Console.WriteLine("=== POSTGRES SAFE SCHEMA SYNC START ===");
    await RunSafePostgresSchemaSyncAsync(db);
    Console.WriteLine("=== POSTGRES SAFE SCHEMA SYNC OK ===");

    Console.WriteLine("=== POSTGRES STARTUP CHECK FINISHED ===");
}

static async Task<bool> TableExistsAsync(AppDbContext db, string tableName)
{
    await using var connection = (NpgsqlConnection)db.Database.GetDbConnection();

    if (connection.State != System.Data.ConnectionState.Open)
    {
        await connection.OpenAsync();
    }

    await using var command = connection.CreateCommand();
    command.CommandText = @"
SELECT EXISTS (
    SELECT 1
    FROM information_schema.tables
    WHERE table_schema = 'public'
      AND table_name = @tableName
);";

    command.Parameters.AddWithValue("@tableName", tableName);

    var result = await command.ExecuteScalarAsync();
    return result is bool exists && exists;
}

static async Task<bool> MigrationExistsInHistoryAsync(AppDbContext db, string migrationId)
{
    await using var connection = (NpgsqlConnection)db.Database.GetDbConnection();

    if (connection.State != System.Data.ConnectionState.Open)
    {
        await connection.OpenAsync();
    }

    await using var command = connection.CreateCommand();
    command.CommandText = @"
SELECT EXISTS (
    SELECT 1
    FROM ""__EFMigrationsHistory""
    WHERE ""MigrationId"" = @migrationId
);";

    command.Parameters.AddWithValue("@migrationId", migrationId);

    var result = await command.ExecuteScalarAsync();
    return result is bool exists && exists;
}

static async Task RunSafePostgresSchemaSyncAsync(AppDbContext db)
{
    await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS ""Advisors"" (
    ""Id"" integer NOT NULL,
    ""Name"" text NOT NULL,
    ""Phone"" text NULL,
    ""Email"" text NULL,
    ""IsActive"" boolean NOT NULL DEFAULT TRUE,
    CONSTRAINT ""PK_Advisors"" PRIMARY KEY (""Id"")
);");

    await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS ""MonthlyReports"" (
    ""Id"" integer NOT NULL,
    ""AdvisorId"" integer NOT NULL,
    ""Year"" integer NOT NULL,
    ""Month"" integer NOT NULL,
    ""Product"" character varying(300) NOT NULL,
    ""Amount"" numeric NOT NULL,
    ""CommissionPercent"" numeric NOT NULL,
    ""Divider"" numeric NOT NULL,
    ""Commission"" numeric NOT NULL,
    ""Su"" numeric NOT NULL,
    ""IsUkContract"" boolean NOT NULL DEFAULT FALSE,
    ""ContractStartDate"" timestamp without time zone NULL,
    ""IsPremiumPaid"" boolean NOT NULL DEFAULT FALSE,
    ""Notes"" character varying(1000) NULL,
    CONSTRAINT ""PK_MonthlyReports"" PRIMARY KEY (""Id"")
);");

    await db.Database.ExecuteSqlRawAsync(@"
ALTER TABLE ""MonthlyReports""
ADD COLUMN IF NOT EXISTS ""ContractStartDate"" timestamp without time zone NULL;");

    await db.Database.ExecuteSqlRawAsync(@"
ALTER TABLE ""MonthlyReports""
ADD COLUMN IF NOT EXISTS ""IsPremiumPaid"" boolean NOT NULL DEFAULT FALSE;");

    await db.Database.ExecuteSqlRawAsync(@"
ALTER TABLE ""MonthlyReports""
ADD COLUMN IF NOT EXISTS ""Notes"" character varying(1000) NULL;");

    await db.Database.ExecuteSqlRawAsync(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'FK_MonthlyReports_Advisors_AdvisorId'
    ) THEN
        ALTER TABLE ""MonthlyReports""
        ADD CONSTRAINT ""FK_MonthlyReports_Advisors_AdvisorId""
        FOREIGN KEY (""AdvisorId"") REFERENCES ""Advisors""(""Id"")
        ON DELETE CASCADE;
    END IF;
END $$;");

    await db.Database.ExecuteSqlRawAsync(@"
CREATE INDEX IF NOT EXISTS ""IX_MonthlyReports_AdvisorId""
ON ""MonthlyReports"" (""AdvisorId"");");
}

sealed class DatabaseConnectionInfo
{
    public bool UsePostgres { get; set; }
    public string Source { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}