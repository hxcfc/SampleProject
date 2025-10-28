using Microsoft.EntityFrameworkCore;
using SampleProject.Domain.Entities;
using SampleProject.Domain.Enums;
using Serilog;
using Common.Shared;

namespace SampleProject.Persistence.Data
{
    /// <summary>
    /// Application database context for Entity Framework Core
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class
        /// </summary>
        /// <param name="options">Database context options</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Users table
        /// </summary>
        public DbSet<UserEntity> Users { get; set; } = null!;

        /// <summary>
        /// User audit logs table
        /// </summary>
        public DbSet<UserAuditLog> UserAuditLogs { get; set; } = null!;

        /// <summary>
        /// Configures entity relationships and constraints
        /// </summary>
        /// <param name="modelBuilder">Model builder instance</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<UserEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordSalt).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Configure Role as enum flags
                entity.Property(e => e.Role)
                    .HasConversion<int>()
                    .HasDefaultValue(UserRole.User);

                // Configure RefreshTokenUseCount
                entity.Property(e => e.RefreshTokenUseCount).HasDefaultValue(0);
            });

            // Configure UserAuditLog entity
            modelBuilder.Entity<UserAuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FieldName).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes for better query performance
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.ChangedByUserId);
            });

            Log.Information(StringMessages.DatabaseContextConfiguredSuccessfully);
        }
    }
}