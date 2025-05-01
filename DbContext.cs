using Construction_Expert.Models;
using Microsoft.EntityFrameworkCore;

public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> o) : base(o) { }

    public DbSet<ConstructionCategory> ConstructionCategories => Set<ConstructionCategory>();
    public DbSet<ConstructionCategoryRelation> ConstructionCategoryRelations => Set<ConstructionCategoryRelation>();
    public DbSet<ConstructionArea> ConstructionAreas => Set<ConstructionArea>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<ConstructionCategory>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(250);
            e.Property(x => x.Name).HasMaxLength(1000);
            e.Property(x => x.Description).HasMaxLength(1000);
        });

        b.Entity<ConstructionCategoryRelation>(e =>
        {
            e.HasKey(x => x.Id);

            e.HasOne(x => x.Category)
              .WithMany(c => c.ParentRelations)
              .HasForeignKey(x => x.CategoryId)
              .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.ParentCategory)
              .WithMany(c => c.ChildRelations)
              .HasForeignKey(x => x.ParentCategoryId)
              .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
