using Microsoft.EntityFrameworkCore;

namespace DV8.Console.Data
{
    public class DV8DbContext : DbContext
    {
        public DV8DbContext(DbContextOptions<DV8DbContext> options) : base(options)
        {
        }

        public DbSet<Device> Devices { get; set; }
        public DbSet<Interface> Interfaces { get; set; }
        public DbSet<Neighbor> Neighbors { get; set; }
        public DbSet<DiscoveryJob> DiscoveryJobs { get; set; }
        public DbSet<DiscoveryResult> DiscoveryResults { get; set; }
        public DbSet<SecretField> SecretFields { get; set; }
        public DbSet<DbgSession> DbgSessions { get; set; }
        public DbSet<CcpDecision> CcpDecisions { get; set; }
        public DbSet<HoacEvent> HoacEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Device
            modelBuilder.Entity<Device>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Fingerprint).IsUnique();
            });

            // Configure Interface
            modelBuilder.Entity<Interface>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Device)
                    .WithMany(e => e.Interfaces)
                    .HasForeignKey(e => e.DeviceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Neighbor (composite key)
            modelBuilder.Entity<Neighbor>(entity =>
            {
                entity.HasKey(e => new { e.AIfId, e.ZIfId, e.Proto });
                
                entity.HasOne(e => e.AInterface)
                    .WithMany(e => e.NeighborsA)
                    .HasForeignKey(e => e.AIfId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.ZInterface)
                    .WithMany(e => e.NeighborsZ)
                    .HasForeignKey(e => e.ZIfId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure DiscoveryJob
            modelBuilder.Entity<DiscoveryJob>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            // Configure DiscoveryResult (composite key)
            modelBuilder.Entity<DiscoveryResult>(entity =>
            {
                entity.HasKey(e => new { e.JobId, e.Fingerprint });
                
                entity.HasOne(e => e.Job)
                    .WithMany(e => e.Results)
                    .HasForeignKey(e => e.JobId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure SecretField
            modelBuilder.Entity<SecretField>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Device)
                    .WithMany(e => e.SecretFields)
                    .HasForeignKey(e => e.DeviceId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure DbgSession
            modelBuilder.Entity<DbgSession>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            // Configure CcpDecision
            modelBuilder.Entity<CcpDecision>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            // Configure HoacEvent
            modelBuilder.Entity<HoacEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Device)
                    .WithMany(e => e.HoacEvents)
                    .HasForeignKey(e => e.DeviceId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}