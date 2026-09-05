using Microsoft.EntityFrameworkCore;
using realworld_net.Entities;

namespace realworld_net.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Follows> Follows { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<Favorites> Favorites { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Follows>(b =>
        {
            b.HasOne(f => f.Follower)
                .WithMany()
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(f => f.Followee)
                .WithMany()
                .HasForeignKey(f => f.FolloweeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Favorites>(b =>
        {
            b.HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(f => f.Article)
                .WithMany()
                .HasForeignKey(f => f.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Auditable && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var auditableEntity = (Auditable)entry.Entity;
            var now = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                auditableEntity.CreatedAt = now;
            }

            auditableEntity.UpdatedAt = now;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
