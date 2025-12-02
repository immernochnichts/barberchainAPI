using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;

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

        public DbSet<ReviewReport> ReviewReports { get; set; }
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
            modelBuilder.Entity<OrderJob>().ToTable("order_job");
            modelBuilder.Entity<ReviewReport>().ToTable("review_report");
            modelBuilder.Entity<ScheduleRequest>().ToTable("schedule_request");
            modelBuilder.Entity<ActionLog>().ToTable("action_log");

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

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(o => o.Status)
                      .HasColumnType("order_status");

                entity.Property(o => o.Method)
                      .HasColumnType("order_method");

                entity.Property(o => o.OrderTime)
                  .HasColumnType("timestamp without time zone")
                  .HasConversion(
                      v => v,
                      v => DateTime.SpecifyKind(v, DateTimeKind.Local)
                  );

                entity.Property(o => o.AppointedTime)
                    .HasColumnType("timestamp without time zone")
                    .HasConversion(
                        v => v,
                        v => DateTime.SpecifyKind(v, DateTimeKind.Local)
                    );

                entity.Property(o => o.PendingUntil)
                    .HasColumnType("timestamp without time zone")
                    .HasConversion(
                        v => v,
                        v => DateTime.SpecifyKind(v.Value, DateTimeKind.Local)
                    );
            });

            modelBuilder.Entity<Review>(entity => entity
                .Property(r => r.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasConversion(
                        v => v,
                        v => DateTime.SpecifyKind(v, DateTimeKind.Local)
                    ));

            modelBuilder.Entity<ReviewReport>(entity => entity
                .Property(r => r.ReportedAt)
                .HasColumnType("timestamp without time zone")
                .HasConversion(
                        v => v,
                        v => DateTime.SpecifyKind(v, DateTimeKind.Local)
                    ));

            modelBuilder.Entity<Notification>(entity => entity
                .Property(r => r.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasConversion(
                        v => v,
                        v => DateTime.SpecifyKind(v, DateTimeKind.Local)
                    ));

            modelBuilder.Entity<Account>(entity =>
                entity.Property(e => e.BirthDate)
                    .HasColumnType("date"));

            modelBuilder.Entity<Account>(entity =>
                entity.Property(e => e.RegTime)
                .HasColumnType("timestamp without time zone")
                .HasConversion(
                        v => v,
                        v => DateTime.SpecifyKind(v, DateTimeKind.Local))
                );

            modelBuilder.Entity<ScheduleRequest>(entity =>
            {
                entity.Property(sr => sr.CreatedAt)
                    .HasColumnType("timestamp without time zone")
                    .HasConversion(
                        v => v,
                        v => DateTime.SpecifyKind(v, DateTimeKind.Local)
                    );
            });

            modelBuilder.Entity<ActionLog>(entity =>
            {
                entity.Property(al => al.CreatedAt)
                    .HasColumnType("timestamp without time zone")
                    .HasConversion(
                        v => v,
                        v => DateTime.SpecifyKind(v, DateTimeKind.Local)
                    );
            });

            modelBuilder.Entity<Complaint>()
                .Property(c => c.Status)
                .HasColumnType("complaint_status");

            modelBuilder.Entity<ScheduleRequest>()
                .Property(sr => sr.Status)
                .HasColumnType("schedule_request_status");

            modelBuilder.Entity<ActionLog>()
                .Property(al => al.ActionType)
                .HasColumnType("action_type");

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
