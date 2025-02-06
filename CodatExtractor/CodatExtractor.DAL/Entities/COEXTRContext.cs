using CodatExtractor.DAL.Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodatExtractor.DAL.Entities
{
    // the EF context for accessing data in DB
    public class COEXTRContext : DbContext
    {
        private string _connectionString;

        // global and per-run error logs
        public virtual DbSet<ErrorLogEntity> ErrorLogs { get; set; }
        // stores individual runs and their success state
        public virtual DbSet<RunEntity> Runs { get; set; }
        // stores company periods for each run
        public virtual DbSet<RunCompanyPeriodEntity> RunCompanyPeriods { get; set; }
        // stores generated CSV informatino for each run
        public virtual DbSet<RunCSVEntity> RunCSVs { get; set; }
        // stores Codat company mappings
        public virtual DbSet<CompanyMappingEntity> CompanyMappings { get; set; }
        // stores user accounts used by the admin page
        public virtual DbSet<UserAccountEntity> UserAccounts { get; set; }
        // stores schedule settings for runs to be performed at given intervals
        public virtual DbSet<ScheduledRunEntity> ScheduledRuns { get; set; }
        // stores shopify stores data and tokens
        public virtual DbSet<ShopifyShopEntity> ShopifyShops { get; set; }
        // stores Shopify mappings
        public virtual DbSet<ShopifyMappingEntity> ShopifyMappings { get; set; }
        // stores Stripe mappings
        public virtual DbSet<StripeMappingEntity> StripeMappings { get; set; }

        // stores Taxually Credentials
        public virtual DbSet<TaxuallyCredentials> TaxuallyCredentials { get; set; }

        public COEXTRContext() { }

        public string DisposeAndRefresh()
        {
            Dispose();
            return _connectionString;
        }

        public COEXTRContext(string connString)
        {
            this._connectionString = connString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseLazyLoadingProxies()
                    .UseSqlServer(_connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ScheduledRunEntity>(entity =>
            {
                entity.HasKey(e => e.ID);

                entity.Property(e => e.ID).ValueGeneratedOnAdd();

                entity.ToTable("COEXTR_ScheduledRuns");
            });

            modelBuilder.Entity<UserAccountEntity>(entity =>
            {
                entity.HasKey(e => e.Username);

                entity.Property(e => e.Username).ValueGeneratedNever();

                entity.ToTable("COEXTR_UserAccounts");
            });

            modelBuilder.Entity<ErrorLogEntity>(entity =>
            {
                entity.HasKey(e => e.ID);

                entity.Property(e => e.ID).ValueGeneratedOnAdd();

                entity.ToTable("COEXTR_ErrorLogs");
            });

            modelBuilder.Entity<RunEntity>(entity =>
            {
                entity.HasKey(e => e.RunTimestamp);

                entity.Property(e => e.InvokedByUser).IsRequired(false);
                entity.Property(e => e.RunForCompany).IsRequired(false);
                entity.Property(e => e.RunForPeriod).IsRequired(false);

                entity.HasMany(e => e.RunCSVs)
                    .WithOne(e => e.Run)
                    .HasForeignKey(e => e.RunTimestamp);

                entity.HasMany(e => e.RunCompanyPeriods)
                    .WithOne(e => e.Run)
                    .HasForeignKey(e => e.RunTimestamp);

                entity.ToTable("COEXTR_Runs");
            });

            modelBuilder.Entity<RunCSVEntity>(entity =>
            {
                entity.HasKey(e => new { e.RunTimestamp, e.TaxuallyCompanyID, e.PeriodID });

                entity.HasOne(e => e.Run)
                    .WithMany(e => e.RunCSVs)
                    .HasForeignKey(e => e.RunTimestamp);

                entity.ToTable("COEXTR_RunCSV");
            });

            modelBuilder.Entity<RunCompanyPeriodEntity>(entity =>
            {
                entity.HasKey(e => new { e.RunTimestamp, e.TaxuallyCompanyID, e.PeriodID });

                entity.HasOne(e => e.Run)
                    .WithMany(e => e.RunCompanyPeriods)
                    .HasForeignKey(e => e.RunTimestamp);

                entity.Property(e => e.IsPeriodProcessed).IsRequired(false);
                entity.Property(e => e.ProcessingStartedAt).IsRequired(false);
                entity.Property(e => e.LastDataID).IsRequired(false);
                entity.ToTable("COEXTR_RunCompanyPeriods");
            });

            modelBuilder.Entity<CompanyMappingEntity>(entity =>
            {
                entity.HasKey(e => new { e.TaxuallyCompanyID, e.CodatCompanyID });

                entity.Property(e => e.CreatedByUser).IsRequired(false);

                entity.ToTable("COEXTR_CompanyMappings");
            });

            modelBuilder.Entity<ShopifyMappingEntity>(entity =>
            {
                entity.HasKey(e => new { e.TaxuallyCompanyID, e.ShopifyShopID });

                entity.Property(e => e.CreatedByUser).IsRequired(false);
                entity.HasOne(e => e.ShopifyShop)
                    .WithMany(e => e.ShopifyMappings)
                    .HasForeignKey(e => e.ShopifyShopID);

                entity.ToTable("SHOPIFY_ShopifyMappings");
            });

            modelBuilder.Entity<StripeMappingEntity>(entity =>
            {
                entity.HasKey(e => new { e.TaxuallyCompanyID, e.StripeAccountId });

                entity.Property(e => e.CreatedByUser).IsRequired(false);

                entity.ToTable("STRIPE_StripeMappings");
            });

            modelBuilder.Entity<ShopifyShopEntity>(entity =>
            {
                entity.HasKey(e => e.ShopID);

                entity.ToTable("SHOPIFY_Shops");
            });

            modelBuilder.Entity<TaxuallyCredentials>(entity =>
            {
                entity.HasKey(e => e.TaxuallyUser);
                entity.Property(e => e.TaxuallyUser).ValueGeneratedNever();

                entity.ToTable("COEXTR_TaxuallyCredentials");
            });
        }
    }
}
