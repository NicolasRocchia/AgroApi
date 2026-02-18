using APIAgroConnect.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace APIAgroConnect.Infrastructure.Data
{
    public class AgroDbContext : DbContext
    {
        public AgroDbContext(DbContextOptions<AgroDbContext> options) : base(options) { }

        // Auth
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRoles> UserRoles => Set<UserRoles>();

        // Dominio Agro
        public DbSet<Requester> Requesters => Set<Requester>();
        public DbSet<Advisor> Advisors => Set<Advisor>();
        public DbSet<Recipe> Recipes => Set<Recipe>();
        public DbSet<RecipeProduct> RecipeProducts => Set<RecipeProduct>();
        public DbSet<RecipeLot> RecipeLots => Set<RecipeLot>();
        public DbSet<RecipeLotVertex> RecipeLotVertices => Set<RecipeLotVertex>();
        public DbSet<RecipeSensitivePoint> RecipeSensitivePoints => Set<RecipeSensitivePoint>();

        // ✅ Catálogo global
        public DbSet<Product> Products => Set<Product>();

        // Municipal
        public DbSet<Municipality> Municipalities => Set<Municipality>();
        public DbSet<RecipeReviewLog> RecipeReviewLogs => Set<RecipeReviewLog>();
        public DbSet<RecipeMessage> RecipeMessages => Set<RecipeMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");

            /* ==========================================================
             * USERS
             * ========================================================== */
            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("Users");
                b.HasKey(x => x.Id);

                b.Property(x => x.UserName).HasMaxLength(100).IsRequired();
                b.Property(x => x.EmailNormalized).HasMaxLength(256).IsRequired();
                b.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();

                b.Property(x => x.PhoneNumber).HasMaxLength(30);
                b.Property(x => x.TaxId).HasMaxLength(20);

                b.Property(x => x.IsBlocked).HasDefaultValue(false).IsRequired();
                b.Property(x => x.CreatedAt).HasDefaultValueSql("sysutcdatetime()").IsRequired();

                b.HasQueryFilter(x => x.DeletedAt == null);

                b.HasIndex(x => x.TaxId)
                    .IsUnique()
                    .HasFilter("[TaxId] IS NOT NULL AND [DeletedAt] IS NULL");

                b.HasOne<User>().WithMany()
                    .HasForeignKey(x => x.CreatedByUserId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Users_CreatedByUser");

                b.HasOne<User>().WithMany()
                    .HasForeignKey(x => x.UpdatedByUserId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Users_UpdatedByUser");

                b.HasOne<User>().WithMany()
                    .HasForeignKey(x => x.DeletedByUserId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Users_DeletedByUser");
            });

            /* ==========================================================
             * ROLES
             * ========================================================== */
            modelBuilder.Entity<Role>(b =>
            {
                b.ToTable("Roles");
                b.HasKey(x => x.Id);

                b.Property(x => x.Name).HasMaxLength(100).IsRequired();
                b.Property(x => x.AccessLevel).HasDefaultValue((short)0).IsRequired();
                b.Property(x => x.Description).HasMaxLength(300);

                b.Property(x => x.CreatedAt).HasDefaultValueSql("sysutcdatetime()").IsRequired();
                b.HasQueryFilter(x => x.DeletedAt == null);

                b.HasOne<User>().WithMany()
                    .HasForeignKey(x => x.CreatedByUserId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Roles_CreatedByUser");

                b.HasOne<User>().WithMany()
                    .HasForeignKey(x => x.UpdatedByUserId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Roles_UpdatedByUser");

                b.HasOne<User>().WithMany()
                    .HasForeignKey(x => x.DeletedByUserId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Roles_DeletedByUser");
            });

            /* ==========================================================
             * USER ROLES
             * ========================================================== */
            modelBuilder.Entity<UserRoles>(b =>
            {
                b.ToTable("UserRoles");
                b.HasKey(x => new { x.UserId, x.RoleId });

                b.Property(x => x.CreatedAt).HasDefaultValueSql("sysutcdatetime()").IsRequired();

                b.HasOne(x => x.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_UserRoles_Users");

                b.HasOne(x => x.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_UserRoles_Roles");
            });

            /* ==========================================================
             * REQUESTERS
             * ========================================================== */
            modelBuilder.Entity<Requester>(b =>
            {
                b.ToTable("Requesters");
                b.HasKey(x => x.Id);

                b.Property(x => x.LegalName).HasMaxLength(200).IsRequired();
                b.Property(x => x.TaxId).HasMaxLength(20).IsRequired();
                b.Property(x => x.Address).HasMaxLength(300);
                b.Property(x => x.Contact).HasMaxLength(200);

                b.Property(x => x.CreatedAt).HasDefaultValueSql("sysutcdatetime()").IsRequired();
                b.HasQueryFilter(x => x.DeletedAt == null);

                b.HasIndex(x => x.TaxId)
                    .IsUnique()
                    .HasFilter("[DeletedAt] IS NULL");

                b.HasOne<User>().WithMany()
                    .HasForeignKey(x => x.CreatedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne<User>().WithMany()
                    .HasForeignKey(x => x.UpdatedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne<User>().WithMany()
                    .HasForeignKey(x => x.DeletedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            /* ==========================================================
             * ADVISORS
             * ========================================================== */
            modelBuilder.Entity<Advisor>(b =>
            {
                b.ToTable("Advisors");
                b.HasKey(x => x.Id);

                b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
                b.Property(x => x.LicenseNumber).HasMaxLength(50).IsRequired();
                b.Property(x => x.Contact).HasMaxLength(200);

                b.Property(x => x.CreatedAt).HasDefaultValueSql("sysutcdatetime()").IsRequired();
                b.HasQueryFilter(x => x.DeletedAt == null);

                b.HasIndex(x => x.LicenseNumber)
                    .IsUnique()
                    .HasFilter("[DeletedAt] IS NULL");

                b.HasOne<User>().WithMany()
                    .HasForeignKey(x => x.CreatedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne<User>().WithMany()
                    .HasForeignKey(x => x.UpdatedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne<User>().WithMany()
                    .HasForeignKey(x => x.DeletedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            /* ==========================================================
             * RECIPES
             * ========================================================== */
            modelBuilder.Entity<Recipe>(b =>
            {
                // ✅ FIX TRIGGERS: EF Core necesita saberlos para no usar OUTPUT en DML
                b.ToTable("Recipes", tb =>
                {
                    tb.HasTrigger("TR_Recipes_StatusHistory");
                    tb.HasTrigger("TR_Recipes_StatusHistory_Insert");
                });

                b.HasKey(x => x.Id);

                b.Property(x => x.Status).HasMaxLength(30).IsRequired();

                b.Property(x => x.ApplicationType).HasMaxLength(100);
                b.Property(x => x.Crop).HasMaxLength(150);
                b.Property(x => x.Diagnosis).HasMaxLength(150);
                b.Property(x => x.Treatment).HasMaxLength(150);
                b.Property(x => x.MachineToUse).HasMaxLength(100);

                b.Property(x => x.MachinePlate).HasMaxLength(50);
                b.Property(x => x.MachineLegalName).HasMaxLength(200);
                b.Property(x => x.MachineType).HasMaxLength(100);

                b.Property(x => x.WindDirection).HasMaxLength(50);

                b.Property(x => x.CreatedAt).HasDefaultValueSql("sysutcdatetime()").IsRequired();
                b.HasQueryFilter(x => x.DeletedAt == null);

                b.HasIndex(x => x.RfdNumber)
                    .IsUnique()
                    .HasFilter("[DeletedAt] IS NULL");

                b.HasOne(x => x.Requester)
                    .WithMany()
                    .HasForeignKey(x => x.RequesterId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(x => x.Advisor)
                    .WithMany()
                    .HasForeignKey(x => x.AdvisorId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne<User>().WithMany()
                    .HasForeignKey(x => x.CreatedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne<User>().WithMany()
                    .HasForeignKey(x => x.UpdatedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne<User>().WithMany()
                    .HasForeignKey(x => x.DeletedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(x => x.AssignedMunicipality)
                    .WithMany()
                    .HasForeignKey(x => x.AssignedMunicipalityId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Recipes_Municipalities");

                b.HasIndex(x => x.AssignedMunicipalityId)
                    .HasFilter("[AssignedMunicipalityId] IS NOT NULL");
            });

            /* ==========================================================
             * MUNICIPALITIES
             * ========================================================== */
            modelBuilder.Entity<Municipality>(b =>
            {
                b.ToTable("Municipalities");
                b.HasKey(x => x.Id);

                b.Property(x => x.Name).HasMaxLength(200).IsRequired();
                b.Property(x => x.Province).HasMaxLength(100);
                b.Property(x => x.Department).HasMaxLength(150);
                b.Property(x => x.Latitude).HasPrecision(10, 7);
                b.Property(x => x.Longitude).HasPrecision(10, 7);

                b.Property(x => x.CreatedAt).HasDefaultValueSql("sysutcdatetime()").IsRequired();
                b.HasQueryFilter(x => x.DeletedAt == null);

                b.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Municipalities_Users");

                b.HasIndex(x => x.UserId)
                    .IsUnique()
                    .HasFilter("[DeletedAt] IS NULL AND [UserId] IS NOT NULL");

                b.HasIndex(x => x.Name)
                    .IsUnique()
                    .HasFilter("[DeletedAt] IS NULL");
            });

            /* ==========================================================
             * RECIPE REVIEW LOG
             * ========================================================== */
            modelBuilder.Entity<RecipeReviewLog>(b =>
            {
                b.ToTable("RecipeReviewLog");
                b.HasKey(x => x.Id);

                b.Property(x => x.Action).HasMaxLength(30).IsRequired();
                b.Property(x => x.Observation).HasMaxLength(500);
                b.Property(x => x.CreatedAt).HasDefaultValueSql("sysutcdatetime()").IsRequired();

                b.HasOne(x => x.Recipe)
                    .WithMany(r => r.ReviewLogs)
                    .HasForeignKey(x => x.RecipeId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(x => x.Municipality)
                    .WithMany()
                    .HasForeignKey(x => x.MunicipalityId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(x => x.TargetMunicipality)
                    .WithMany()
                    .HasForeignKey(x => x.TargetMunicipalityId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasIndex(x => new { x.RecipeId, x.CreatedAt });
                b.HasIndex(x => x.MunicipalityId);
            });

            /* ==========================================================
             * RECIPE MESSAGES
             * ========================================================== */
            modelBuilder.Entity<RecipeMessage>(b =>
            {
                b.ToTable("RecipeMessages");
                b.HasKey(x => x.Id);

                b.Property(x => x.Message).HasMaxLength(1000).IsRequired();
                b.Property(x => x.CreatedAt).HasDefaultValueSql("sysutcdatetime()").IsRequired();

                b.HasOne(x => x.Recipe)
                    .WithMany(r => r.Messages)
                    .HasForeignKey(x => x.RecipeId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(x => x.Sender)
                    .WithMany()
                    .HasForeignKey(x => x.SenderUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasIndex(x => new { x.RecipeId, x.CreatedAt });
            });

            /* ==========================================================
             * PRODUCTS (CATÁLOGO GLOBAL)
             * ========================================================== */
            modelBuilder.Entity<Product>(b =>
            {
                b.ToTable("Products");
                b.HasKey(x => x.Id);

                b.Property(x => x.SenasaRegistry).HasMaxLength(50);
                b.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
                b.Property(x => x.ToxicologicalClass).HasMaxLength(100);

                b.Property(x => x.CreatedAt).HasDefaultValueSql("sysutcdatetime()").IsRequired();
                b.HasQueryFilter(x => x.DeletedAt == null);

                b.HasIndex(x => x.SenasaRegistry)
                    .IsUnique()
                    .HasFilter("[DeletedAt] IS NULL AND [SenasaRegistry] IS NOT NULL");
            });

            /* ==========================================================
             * RECIPE PRODUCTS (PUENTE)
             * ========================================================== */
            modelBuilder.Entity<RecipeProduct>(b =>
            {
                b.ToTable("RecipeProducts");
                b.HasKey(x => x.Id);

                b.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
                b.Property(x => x.SenasaRegistry).HasMaxLength(50);
                b.Property(x => x.ToxicologicalClass).HasMaxLength(100);

                b.Property(x => x.ProductType).HasMaxLength(50);

                b.Property(x => x.DoseValue).HasPrecision(18, 6);
                b.Property(x => x.TotalValue).HasPrecision(18, 6);

                b.Property(x => x.DoseUnit).HasMaxLength(30);
                b.Property(x => x.DosePerUnit).HasMaxLength(30);
                b.Property(x => x.TotalUnit).HasMaxLength(30);

                b.Property(x => x.CreatedAt).HasDefaultValueSql("sysutcdatetime()").IsRequired();
                b.HasQueryFilter(x => x.DeletedAt == null);

                b.HasOne(x => x.Recipe)
                    .WithMany(r => r.Products)
                    .HasForeignKey(x => x.RecipeId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasOne(x => x.Product)
                    .WithMany(p => p.RecipeProducts)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasIndex(x => new { x.RecipeId, x.ProductId })
                    .IsUnique()
                    .HasFilter("[DeletedAt] IS NULL");
            });

            /* ==========================================================
             * RECIPE LOTS
             * ========================================================== */
            modelBuilder.Entity<RecipeLot>(b =>
            {
                b.ToTable("RecipeLots");
                b.HasKey(x => x.Id);

                b.Property(x => x.LotName).HasMaxLength(200).IsRequired();
                b.Property(x => x.Locality).HasMaxLength(150);
                b.Property(x => x.Department).HasMaxLength(150);
                b.Property(x => x.SurfaceHa).HasPrecision(10, 2);

                b.Property(x => x.CreatedAt).HasDefaultValueSql("sysutcdatetime()").IsRequired();
                b.HasQueryFilter(x => x.DeletedAt == null);

                b.HasOne(x => x.Recipe)
                    .WithMany(r => r.Lots)
                    .HasForeignKey(x => x.RecipeId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            /* ==========================================================
             * RECIPE LOT VERTICES
             * ========================================================== */
            modelBuilder.Entity<RecipeLotVertex>(b =>
            {
                b.ToTable("RecipeLotVertices");
                b.HasKey(x => x.Id);

                b.Property(x => x.Latitude).HasPrecision(10, 7);
                b.Property(x => x.Longitude).HasPrecision(10, 7);

                b.Property(x => x.CreatedAt).HasDefaultValueSql("sysutcdatetime()").IsRequired();

                b.HasOne(x => x.Lot)
                    .WithMany(l => l.Vertices)
                    .HasForeignKey(x => x.LotId)
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasIndex(x => new { x.LotId, x.Order })
                    .IsUnique()
                    .HasFilter("[DeletedAt] IS NULL");

                b.HasQueryFilter(x => x.DeletedAt == null);
            });

            /* ==========================================================
             * RECIPE SENSITIVE POINTS
             * ========================================================== */
            modelBuilder.Entity<RecipeSensitivePoint>(b =>
            {
                b.ToTable("RecipeSensitivePoints");
                b.HasKey(x => x.Id);

                b.Property(x => x.Latitude).HasPrecision(10, 7);
                b.Property(x => x.Longitude).HasPrecision(10, 7);

                b.Property(x => x.Name).HasMaxLength(200).IsRequired();
                b.Property(x => x.Type).HasMaxLength(100);
                b.Property(x => x.Locality).HasMaxLength(150);
                b.Property(x => x.Department).HasMaxLength(150);

                b.Property(x => x.CreatedAt).HasDefaultValueSql("sysutcdatetime()").IsRequired();
                b.HasQueryFilter(x => x.DeletedAt == null);

                b.HasOne(x => x.Recipe)
                    .WithMany(r => r.SensitivePoints)
                    .HasForeignKey(x => x.RecipeId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}