using GeneratorsManagementSystem.Models;
using GeneratorsManagementSystem.Models.Accounting;
using GeneratorsManagementSystem.Models.Fuel;
using GeneratorsManagementSystem.Models.Geography;
using GeneratorsManagementSystem.Models.Identity;
using GeneratorsManagementSystem.Models.IoT;
using GeneratorsManagementSystem.Models.Settings;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GeneratorsManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ═══ Settings ═══
        public DbSet<ThemeSettings> ThemeSettings { get; set; }
        public DbSet<SystemSettings> SystemSettings { get; set; }
        public DbSet<OrganizationSettings> OrganizationSettings { get; set; }
        public DbSet<GeneratorSettings> GeneratorSettings { get; set; }
        public DbSet<SubscriptionSettings> SubscriptionSettings { get; set; }
        public DbSet<BillingSettings> BillingSettings { get; set; }

        public DbSet<GeneratorBook> GeneratorBooks { get; set; }

        // ═══ Geography ═══
        public DbSet<Governorate> Governorates { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Neighborhood> Neighborhoods { get; set; }
        public DbSet<Alley> Alleys { get; set; }

        // ═══ Configurations ═══
        public DbSet<DeviceType> DeviceTypes { get; set; }
        public DbSet<DiscountReason> DiscountReasons { get; set; }

        // أضف هذا:
        public DbSet<Expense> Expenses { get; set; }

        // ═══ Core ═══
        public DbSet<Generator> Generators { get; set; }
        public DbSet<GeneratorLog> GeneratorLogs { get; set; }
        public DbSet<FuelRecord> FuelRecords { get; set; }

        public DbSet<Subscriber> Subscribers { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }

        // أضف هذا في قسم DbSets
        public DbSet<AuditLog> AuditLogs { get; set; }

        // ═══ Fuel ═══
        public DbSet<FuelAllocation> FuelAllocations { get; set; }
        public DbSet<FuelConsumption> FuelConsumptions { get; set; }

        // ═══ Fuel Management ═══
        public DbSet<OperatingSession> OperatingSessions { get; set; }
        public DbSet<FuelRefill> FuelRefills { get; set; }

        // ═══ IoT ═══
        public DbSet<IoTDevice> IoTDevices { get; set; }
        public DbSet<SensorReading> SensorReadings { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ─── Settings ───
            builder.Entity<ThemeSettings>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.UserId).IsRequired();
            });

            builder.Entity<SystemSettings>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.SettingKey).IsRequired().HasMaxLength(100);
                e.Property(x => x.SettingValue).HasMaxLength(500);
            });

            builder.Entity<GeneratorSettings>().HasKey(x => x.Id);
            builder.Entity<SubscriptionSettings>().HasKey(x => x.Id);
            builder.Entity<BillingSettings>().HasKey(x => x.Id);

            builder.Entity<OrganizationSettings>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.OrganizationName).IsRequired().HasMaxLength(200);
            });

            // ─── Generator ───
            builder.Entity<Generator>(e =>
            {
                e.HasIndex(g => g.GeneratorNumber).IsUnique();
            });

            builder.Entity<GeneratorLog>(e =>
            {
                e.HasOne(l => l.Generator)
                 .WithMany(g => g.Logs)
                 .HasForeignKey(l => l.GeneratorId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<FuelRecord>(e =>
            {
                e.HasOne(f => f.Generator)
                 .WithMany(g => g.FuelRecords)
                 .HasForeignKey(f => f.GeneratorId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ─── Subscriber ───
            builder.Entity<Subscriber>(e =>
            {
                e.HasIndex(s => s.SubscriberNumber).IsUnique();
                e.Property(s => s.FullName).IsRequired().HasMaxLength(100);
            });

            // ─── Subscription ───
            builder.Entity<Subscription>(e =>
            {
                e.HasIndex(s => s.ContractNumber).IsUnique();

                e.HasOne(s => s.Subscriber)
                 .WithMany(sub => sub.Subscriptions)
                 .HasForeignKey(s => s.SubscriberId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(s => s.Generator)
                 .WithMany(g => g.Subscriptions)
                 .HasForeignKey(s => s.GeneratorId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ─── Invoice ───
            builder.Entity<Invoice>(e =>
            {
                e.HasIndex(i => i.InvoiceNumber).IsUnique();

                e.HasOne(i => i.Subscription)
                 .WithMany(s => s.Invoices)
                 .HasForeignKey(i => i.SubscriptionId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(i => i.Subscriber)
                 .WithMany(s => s.Invoices)
                 .HasForeignKey(i => i.SubscriberId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ─── Payment ───
            builder.Entity<Payment>(e =>
            {
                e.HasIndex(p => p.ReceiptNumber).IsUnique();

                e.HasOne(p => p.Invoice)
                 .WithMany(i => i.Payments)
                 .HasForeignKey(p => p.InvoiceId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(p => p.Subscriber)
                 .WithMany(s => s.Payments)
                 .HasForeignKey(p => p.SubscriberId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // أضف هذا في OnModelCreating قبل القوس الأخير
            builder.Entity<AuditLog>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Timestamp);
                e.HasIndex(x => x.UserId);
                e.HasIndex(x => x.Module);
                e.HasIndex(x => x.ActionType);
            });

            builder.Entity<Expense>(e =>
            {
                e.HasIndex(x => x.ExpenseNumber).IsUnique();
                e.HasIndex(x => x.ExpenseDate);
                e.HasIndex(x => x.Category);

                e.HasOne(x => x.Generator)
                 .WithMany(g => g.Expenses)
                 .HasForeignKey(x => x.GeneratorId)
                 .OnDelete(DeleteBehavior.SetNull)
                 .IsRequired(false);
            });

            // ─── Fuel Allocations ───
            builder.Entity<FuelAllocation>(e =>
            {
                e.HasIndex(x => x.AllocationNumber).IsUnique();
                e.HasIndex(x => x.AllocationDate);
                e.HasIndex(x => new { x.AllocationYear, x.AllocationMonth });
            });

            // ─── Fuel Consumptions ───
            builder.Entity<FuelConsumption>(e =>
            {
                e.HasIndex(x => x.ConsumptionNumber).IsUnique();
                e.HasIndex(x => x.ConsumptionDate);

                e.HasOne(x => x.Generator)
                 .WithMany(g => g.FuelConsumptions)
                 .HasForeignKey(x => x.GeneratorId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.FuelAllocation)
                 .WithMany(a => a.Consumptions)
                 .HasForeignKey(x => x.FuelAllocationId)
                 .OnDelete(DeleteBehavior.SetNull)
                 .IsRequired(false);
            });

            // ─── Generator Books ───
            builder.Entity<GeneratorBook>(e =>
            {
                e.HasIndex(x => x.InternalNumber).IsUnique();
                e.HasIndex(x => x.BookNumber);
                e.HasIndex(x => x.ExpiryDate);
                e.HasIndex(x => x.Category);

                // Self-Reference بدون Cascade
                e.HasOne(x => x.RenewedFromBook)
                 .WithMany()
                 .HasForeignKey(x => x.RenewedFromBookId)
                 .OnDelete(DeleteBehavior.NoAction)
                 .IsRequired(false);
            });

            // ─── Geography ───
            builder.Entity<Governorate>(e =>
            {
                e.HasIndex(x => x.Name);
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            });

            builder.Entity<District>(e =>
            {
                e.HasIndex(x => x.Name);
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);

                e.HasOne(x => x.Governorate)
                 .WithMany(g => g.Districts)
                 .HasForeignKey(x => x.GovernorateId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Neighborhood>(e =>
            {
                e.HasIndex(x => x.Name);
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);

                e.HasOne(x => x.District)
                 .WithMany(d => d.Neighborhoods)
                 .HasForeignKey(x => x.DistrictId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Alley>(e =>
            {
                e.HasIndex(x => x.Name);
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);

                e.HasOne(x => x.Neighborhood)
                 .WithMany(n => n.Alleys)
                 .HasForeignKey(x => x.NeighborhoodId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ─── DeviceType ───
            builder.Entity<DeviceType>(e =>
            {
                e.HasIndex(x => x.Name).IsUnique();
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);
                e.Property(x => x.DefaultPrice).HasColumnType("decimal(12,2)");
                e.Property(x => x.DefaultAmpere).HasColumnType("decimal(10,2)");
            });

            // ─── DiscountReason ───
            builder.Entity<DiscountReason>(e =>
            {
                e.HasIndex(x => x.Name).IsUnique();
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            });

            // ─── Subscriber العلاقات الجديدة ───
            builder.Entity<Subscriber>(e =>
            {
                e.HasOne(x => x.Governorate)
                 .WithMany()
                 .HasForeignKey(x => x.GovernorateId)
                 .OnDelete(DeleteBehavior.SetNull)
                 .IsRequired(false);

                e.HasOne(x => x.District)
                 .WithMany()
                 .HasForeignKey(x => x.DistrictId)
                 .OnDelete(DeleteBehavior.NoAction)
                 .IsRequired(false);

                e.HasOne(x => x.Neighborhood)
                 .WithMany()
                 .HasForeignKey(x => x.NeighborhoodId)
                 .OnDelete(DeleteBehavior.NoAction)
                 .IsRequired(false);

                e.HasOne(x => x.Alley)
                 .WithMany()
                 .HasForeignKey(x => x.AlleyId)
                 .OnDelete(DeleteBehavior.NoAction)
                 .IsRequired(false);
            });

            // ─── Subscription العلاقات الجديدة ───
            builder.Entity<Subscription>(e =>
            {
                e.HasOne(x => x.DeviceType)
                 .WithMany()
                 .HasForeignKey(x => x.DeviceTypeId)
                 .OnDelete(DeleteBehavior.SetNull)
                 .IsRequired(false);

                e.HasOne(x => x.DiscountReason)
                 .WithMany()
                 .HasForeignKey(x => x.DiscountReasonId)
                 .OnDelete(DeleteBehavior.SetNull)
                 .IsRequired(false);
            });


            // ═══ OperatingSession ═══
            builder.Entity<OperatingSession>(entity =>
            {
                entity.HasOne(x => x.Generator)
                      .WithMany(g => g.OperatingSessions)
                      .HasForeignKey(x => x.GeneratorId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => x.StartTime);
                entity.HasIndex(x => x.GeneratorId);
            });

            // ═══ FuelRefill ═══
            builder.Entity<FuelRefill>(entity =>
            {
                entity.HasOne(x => x.Generator)
                      .WithMany(g => g.FuelRefills)
                      .HasForeignKey(x => x.GeneratorId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.FuelAllocation)
                      .WithMany()
                      .HasForeignKey(x => x.FuelAllocationId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(x => x.RefillDate);
                entity.HasIndex(x => x.RefillNumber).IsUnique();
            });

            // ═══ IoTDevice ═══
            builder.Entity<IoTDevice>(entity =>
            {
                entity.HasOne(x => x.Generator)
                      .WithMany(g => g.IoTDevices)
                      .HasForeignKey(x => x.GeneratorId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => x.ApiKey).IsUnique();
                entity.HasIndex(x => x.DeviceName).IsUnique();
                entity.HasIndex(x => x.Status);
            });

            // ═══ SensorReading ═══
            builder.Entity<SensorReading>(entity =>
            {
                entity.HasOne(x => x.IoTDevice)
                      .WithMany(d => d.SensorReadings)
                      .HasForeignKey(x => x.IoTDeviceId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Generator)
                      .WithMany()
                      .HasForeignKey(x => x.GeneratorId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(x => x.ReadingTime);
                entity.HasIndex(x => new { x.GeneratorId, x.ReadingType });
            });

            // ═══ FuelAllocation - إضافة العلاقة مع Generator ═══
            builder.Entity<FuelAllocation>()
                .HasOne(x => x.Generator)
                .WithMany()
                .HasForeignKey(x => x.GeneratorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}