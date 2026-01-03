using APIAgroConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace APIAgroConnect.Infrastructure.Data
{
    public class AgroDbContext : DbContext
    {
        public AgroDbContext(DbContextOptions<AgroDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();

        public DbSet<Requester> Requesters => Set<Requester>();
        public DbSet<Advisor> Advisors => Set<Advisor>();
        public DbSet<Recipe> Recipes => Set<Recipe>();
        public DbSet<RecipeProduct> RecipeProducts => Set<RecipeProduct>();
        public DbSet<RecipeLot> RecipeLots => Set<RecipeLot>();
        public DbSet<RecipeLotVertex> RecipeLotVertices => Set<RecipeLotVertex>();
        public DbSet<RecipeSensitivePoint> RecipeSensitivePoints => Set<RecipeSensitivePoint>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /* =========================
               USER <-> ROLE (M:N)
               ========================= */
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.NoAction);

            /* =========================
               RECIPES RELATIONS
               ========================= */
            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.Requester)
                .WithMany()
                .HasForeignKey(r => r.RequesterId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.Advisor)
                .WithMany()
                .HasForeignKey(r => r.AdvisorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RecipeProduct>()
                .HasOne(p => p.Recipe)
                .WithMany(r => r.Products)
                .HasForeignKey(p => p.RecipeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RecipeLot>()
                .HasOne(l => l.Recipe)
                .WithMany(r => r.Lots)
                .HasForeignKey(l => l.RecipeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RecipeLotVertex>()
                .HasOne(v => v.Lot)
                .WithMany(l => l.Vertices)
                .HasForeignKey(v => v.LotId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RecipeSensitivePoint>()
                .HasOne(s => s.Recipe)
                .WithMany(r => r.SensitivePoints)
                .HasForeignKey(s => s.RecipeId)
                .OnDelete(DeleteBehavior.NoAction);

            /* =========================
               INDICES / UNIQUES
               ========================= */

            modelBuilder.Entity<Recipe>()
                .HasIndex(r => r.RfdNumber)
                .IsUnique()
                .HasFilter("[DeletedAt] IS NULL");

            modelBuilder.Entity<Requester>()
                .HasIndex(r => r.TaxId)
                .IsUnique()
                .HasFilter("[DeletedAt] IS NULL");

            modelBuilder.Entity<Advisor>()
                .HasIndex(a => a.LicenseNumber)
                .IsUnique()
                .HasFilter("[DeletedAt] IS NULL");

            modelBuilder.Entity<RecipeLotVertex>()
                .HasIndex(v => new { v.LotId, v.Order })
                .IsUnique()
                .HasFilter("[DeletedAt] IS NULL");

            modelBuilder.Entity<Recipe>()
                .Property(r => r.IssueDate)
                .HasColumnType("date");

            modelBuilder.Entity<Recipe>()
                .Property(r => r.PossibleStartDate)
                .HasColumnType("date");

            modelBuilder.Entity<Recipe>()
                .Property(r => r.RecommendedDate)
                .HasColumnType("date");

            modelBuilder.Entity<Recipe>()
                .Property(r => r.ExpirationDate)
                .HasColumnType("date");

            modelBuilder.Entity<Recipe>()
                .Property(r => r.UnitSurfaceHa)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<RecipeLot>()
                .Property(l => l.SurfaceHa)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<RecipeLotVertex>()
                .Property(v => v.Latitude)
                .HasColumnType("decimal(10,7)");

            modelBuilder.Entity<RecipeLotVertex>()
                .Property(v => v.Longitude)
                .HasColumnType("decimal(10,7)");

            /* =========================
               GLOBAL SOFT DELETE FILTERS
               =========================
            */

            modelBuilder.Entity<Requester>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<Advisor>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<Recipe>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<RecipeProduct>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<RecipeLot>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<RecipeLotVertex>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuilder.Entity<RecipeSensitivePoint>().HasQueryFilter(e => e.DeletedAt == null);

        }
    }
}
