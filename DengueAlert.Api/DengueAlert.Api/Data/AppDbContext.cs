using DengueAlert.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DengueAlert.Api.Data
{
    public class AppDbContext : DbContext 
    {
        public DbSet<DengueAlerta> DengueAlerts { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DengueAlerta>()
                .HasKey(d => d.Id);

            modelBuilder.Entity<DengueAlerta>()
                .HasIndex(d => new { d.EndWeek, d.EndYear, d.GeoCode })
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
