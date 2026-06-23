using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VietNhatHospital.Models;

namespace VietNhatHospital.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Drug> Drugs { get; set; }
    public DbSet<Condition> Conditions { get; set; }
    public DbSet<DrugContraindication> DrugContraindications { get; set; }
    public DbSet<DrugInteraction> DrugInteractions { get; set; }
    public DbSet<PatientCondition> PatientConditions { get; set; }
    public DbSet<SearchHistoryItem> SearchHistories { get; set; }
    public DbSet<ReviewItem> ReviewItems { get; set; }
    public DbSet<ErrorReport> ErrorReports { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 1. Configure PatientCondition (Many-to-Many Join Table)
        builder.Entity<PatientCondition>(entity =>
        {
            entity.HasKey(pc => new { pc.UserId, pc.ConditionId });

            entity.HasOne(pc => pc.Patient)
                .WithMany(u => u.PatientConditions)
                .HasForeignKey(pc => pc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pc => pc.Condition)
                .WithMany(c => c.PatientConditions)
                .HasForeignKey(pc => pc.ConditionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 2. Configure DrugContraindications relationships
        builder.Entity<DrugContraindication>(entity =>
        {
            entity.HasKey(dc => dc.Id);

            entity.HasOne(dc => dc.Drug)
                .WithMany(d => d.Contraindications)
                .HasForeignKey(dc => dc.DrugId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(dc => dc.Condition)
                .WithMany(c => c.Contraindications)
                .HasForeignKey(dc => dc.ConditionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 3. Configure DrugInteractions relationships
        builder.Entity<DrugInteraction>(entity =>
        {
            entity.HasKey(di => di.Id);

            entity.HasOne(di => di.Drug1)
                .WithMany()
                .HasForeignKey(di => di.DrugId1)
                .OnDelete(DeleteBehavior.Restrict); // Prevent multiple cascade paths in SQL Server

            entity.HasOne(di => di.Drug2)
                .WithMany()
                .HasForeignKey(di => di.DrugId2)
                .OnDelete(DeleteBehavior.Restrict); // Prevent multiple cascade paths in SQL Server
        });
    }
}
