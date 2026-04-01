using AdvisorDashboardApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AdvisorDashboardApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Advisor> Advisors => Set<Advisor>();
    public DbSet<MonthlyReport> MonthlyReports => Set<MonthlyReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Advisor>()
            .Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        modelBuilder.Entity<Advisor>()
            .Property(x => x.Phone)
            .HasMaxLength(50);

        modelBuilder.Entity<Advisor>()
            .Property(x => x.Email)
            .HasMaxLength(150);

        modelBuilder.Entity<MonthlyReport>()
            .Property(x => x.Product)
            .HasMaxLength(300)
            .IsRequired();

        modelBuilder.Entity<MonthlyReport>()
            .Property(x => x.Notes)
            .HasMaxLength(500);

        modelBuilder.Entity<MonthlyReport>()
            .HasOne(x => x.Advisor)
            .WithMany(x => x.MonthlyReports)
            .HasForeignKey(x => x.AdvisorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}