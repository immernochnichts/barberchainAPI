using Microsoft.EntityFrameworkCore;
using System;

namespace barberchainAPI.Data
{
    public class BarberchainDbContext : DbContext
    {
        public BarberchainDbContext(DbContextOptions<BarberchainDbContext> options)
            : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Barber> Barbers { get; set; }
        public DbSet<Barbershop> Barbershops { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderJob> OrderJobs { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AccountNotification> AccountNotifications { get; set; }
        public DbSet<BarberJob> BarberJobs { get; set; }
        public DbSet<BarberScheduleDay> BarberScheduleDays { get; set; }
        public DbSet<ImageBarber> BarberImages { get; set; }
        public DbSet<ImageBshop> BarbershopImages { get; set; }
        public DbSet<ModerationLog> ModerationLogs { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<ScheduleRequest> ScheduleRequests { get; set; }
        public DbSet<ActionLog> ActionLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Barber>().ToTable("barber");
            modelBuilder.Entity<Account>().ToTable("account");
            modelBuilder.Entity<ImageBarber>().ToTable("images_barber");
            modelBuilder.Entity<BarberScheduleDay>().ToTable("barber_schedule_day");
            modelBuilder.Entity<BarberJob>().ToTable("barber_job");
            modelBuilder.Entity<Job>().ToTable("job");
            modelBuilder.Entity<Barbershop>().ToTable("bshop");
            modelBuilder.Entity<ImageBshop>().ToTable("images_bshop");
            modelBuilder.Entity<Notification>().ToTable("notification");
            modelBuilder.Entity<AccountNotification>().ToTable("account_notification");
            modelBuilder.Entity<Review>().ToTable("review");
            modelBuilder.Entity<Order>().ToTable("order_");

            // Composite key for BarberJob
            modelBuilder.Entity<BarberJob>()
                .HasKey(bj => new { bj.BarberId, bj.JobId });

            // Composite key for OrderJob
            modelBuilder.Entity<OrderJob>()
                .HasKey(oj => new { oj.OrderId, oj.JobId });

            // Composite key for AccountNotification
            modelBuilder.Entity<AccountNotification>()
                .HasKey(an => new { an.AccountId, an.NotificationId });

            // Unique constraint for BarberScheduleDay (BarberId + Date)
            modelBuilder.Entity<BarberScheduleDay>()
                .HasIndex(bsd => new { bsd.BarberId, bsd.Date })
                .IsUnique();

            // Enum conversions (Postgres enums → C# enums)
            modelBuilder.Entity<Order>()
                .Property(o => o.Method)
                .HasConversion<string>();

            modelBuilder.Entity<Complaint>()
                .Property(c => c.Status)
                .HasConversion<string>();

            modelBuilder.Entity<ScheduleRequest>()
                .Property(sr => sr.Status)
                .HasConversion<string>();

            modelBuilder.Entity<ActionLog>()
                .Property(al => al.ActionType)
                .HasConversion<string>();

            modelBuilder.HasPostgresEnum<AccountRole>("account_role");
            modelBuilder.Entity<Account>()
                .Property(a => a.Role)
                .HasColumnType("account_role");
            modelBuilder.Entity<Account>()
                .Property(e => e.Restname)
                .HasMaxLength(256)
                .IsRequired(false); // explicitly nullable

        }
    }
}
