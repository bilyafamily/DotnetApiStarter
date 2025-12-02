using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MobileAPI.Models;

namespace MobileAPI.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Sector> Sectors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // THIS LINE IS CRITICAL - don't forget it!
        base.OnModelCreating(modelBuilder);

        // Your custom configurations
        modelBuilder.Entity<Sector>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.HasIndex(s => s.Name)
                .IsUnique();
        });
    }
}