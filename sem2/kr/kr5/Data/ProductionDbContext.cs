using kr5.Models;
using Microsoft.EntityFrameworkCore;

namespace kr5.Data;

public class ProductionDbContext(DbContextOptions<ProductionDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductionLine> ProductionLines => Set<ProductionLine>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<ProductMaterial> ProductMaterials => Set<ProductMaterial>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(x => x.Name).IsRequired().HasMaxLength(120);
            entity.Property(x => x.Category).HasMaxLength(80);
            entity.Property(x => x.Specifications).HasDefaultValue("{}");
        });

        modelBuilder.Entity<ProductionLine>(entity =>
        {
            entity.Property(x => x.Name).IsRequired().HasMaxLength(120);
            entity.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Stopped");
            entity.Property(x => x.EfficiencyFactor).HasDefaultValue(1f);
            entity.HasOne(x => x.CurrentWorkOrder)
                .WithMany()
                .HasForeignKey(x => x.CurrentWorkOrderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.Property(x => x.Name).IsRequired().HasMaxLength(120);
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.MinimalStock).HasPrecision(18, 3);
            entity.Property(x => x.UnitOfMeasure).HasMaxLength(24);
        });

        modelBuilder.Entity<ProductMaterial>(entity =>
        {
            entity.HasKey(x => new { x.ProductId, x.MaterialId });
            entity.Property(x => x.QuantityNeeded).HasPrecision(18, 3);
            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductMaterials)
                .HasForeignKey(x => x.ProductId);
            entity.HasOne(x => x.Material)
                .WithMany(x => x.ProductMaterials)
                .HasForeignKey(x => x.MaterialId);
        });

        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Pending");
            entity.HasOne(x => x.Product)
                .WithMany(x => x.WorkOrders)
                .HasForeignKey(x => x.ProductId);
            entity.HasOne(x => x.ProductionLine)
                .WithMany(x => x.WorkOrders)
                .HasForeignKey(x => x.ProductionLineId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
