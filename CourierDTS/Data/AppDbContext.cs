using Microsoft.EntityFrameworkCore;
using CourierDTS.Models;

namespace CourierDTS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Admin> Admins => Set<Admin>();
        public DbSet<Courier> Couriers => Set<Courier>();
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<Journey> Journeys => Set<Journey>();
        public DbSet<Package> Packages => Set<Package>();
        public DbSet<PackageHistory> PackageHistories => Set<PackageHistory>();
        public DbSet<TelemetryLog> TelemetryLogs => Set<TelemetryLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Journey iki farklı Location'a bağlanıyor (StartLocation/EndLocation) -
            // EF Core bunu otomatik çözemiyor, ilişkileri açıkça belirtmek gerekiyor.
            modelBuilder.Entity<Journey>()
                .HasOne(j => j.StartLocation)
                .WithMany()
                .HasForeignKey(j => j.StartLocId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Journey>()
                .HasOne(j => j.EndLocation)
                .WithMany()
                .HasForeignKey(j => j.EndLocId)
                .OnDelete(DeleteBehavior.Restrict);

            // Package de aynı şekilde iki farklı Location'a bağlanıyor.
            modelBuilder.Entity<Package>()
                .HasOne(p => p.PickupLocation)
                .WithMany()
                .HasForeignKey(p => p.PickupLocId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Package>()
                .HasOne(p => p.DropoffLocation)
                .WithMany()
                .HasForeignKey(p => p.DropoffLocId)
                .OnDelete(DeleteBehavior.Restrict);

            // Courier.ActiveVehicleId <-> Vehicle.CourierId çift yönlü, birbirinden
            // bağımsız iki ayrı ilişki - EF'in birini diğeriyle karıştırmaması için
            // ikisi de ayrı ayrı, WithMany() ile tanımlanıyor.
            modelBuilder.Entity<Courier>()
                .HasOne(c => c.ActiveVehicle)
                .WithMany()
                .HasForeignKey(c => c.ActiveVehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Courier)
                .WithMany()
                .HasForeignKey(v => v.CourierId)
                .OnDelete(DeleteBehavior.Restrict);

            // PackageHistory asla silinmemeli (chain-of-custody) - ilişkili Journey/Package
            // silinmeye çalışılırsa engellensin diye Restrict, Cascade değil.
            modelBuilder.Entity<PackageHistory>()
                .HasOne(ph => ph.Journey)
                .WithMany()
                .HasForeignKey(ph => ph.JourneyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PackageHistory>()
                .HasOne(ph => ph.Package)
                .WithMany()
                .HasForeignKey(ph => ph.PackageId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
