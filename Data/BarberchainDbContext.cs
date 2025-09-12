using Microsoft.EntityFrameworkCore;
using System.Security.Principal;
using System.Threading;

namespace barberchainAPI.Data
{
    public class BarberchainDbContext : DbContext
    {
        public BarberchainDbContext(DbContextOptions<BarberchainDbContext> options)
        : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Barber> Barbers { get; set; }
        public DbSet<Bshop> Bshops { get; set; }
        public DbSet<BarberScheduleDay> BarberScheduleDays { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<BarberJob> BarberJobs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderJob> OrderJobs { get; set; }
        public DbSet<BshopImage> BshopImages { get; set; }
        public DbSet<BarberImage> BarberImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enum mapping (PostgreSQL)
            modelBuilder.HasPostgresEnum<AccountRole>();
            modelBuilder.HasPostgresEnum<OrderStatus>();

            // Account
            modelBuilder.Entity<Account>()
                .Property(a => a.Role)
                .HasConversion<string>();

            // One-to-one: Barber ↔ Account
            modelBuilder.Entity<Barber>()
            .HasOne(b => b.Account)
            .WithOne()
                .HasForeignKey<Barber>(b => b.FkAccount)
                .OnDelete(DeleteBehavior.Cascade);

            // Bshop → Manager Account (many-to-one)
            modelBuilder.Entity<Bshop>()
                .HasOne(b => b.Manager)
                .WithMany()
                .HasForeignKey(b => b.FkManagerAccount)
                .OnDelete(DeleteBehavior.Restrict);

            // BarberScheduleDay → Created By Manager (many-to-one)
            modelBuilder.Entity<BarberScheduleDay>()
                .HasOne(b => b.CreatedByManager)
                .WithMany()
                .HasForeignKey(b => b.FkManagerAccount)
                .OnDelete(DeleteBehavior.SetNull);

            // BarberJob: composite key
            modelBuilder.Entity<BarberJob>()
                .HasKey(bj => new { bj.FkBarber, bj.FkJob });

            // OrderJobs: FK setup
            modelBuilder.Entity<OrderJob>()
                .HasOne(oj => oj.Order)
                .WithMany(o => o.OrderJobs)
                .HasForeignKey(oj => oj.FkOrder)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderJob>()
                .HasOne(oj => oj.Job)
                .WithMany()
                .HasForeignKey(oj => oj.FkJob)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
