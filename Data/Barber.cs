using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace barberchainAPI.Data
{
    public class Barber
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("fk_bshop")]
        public int? BshopId { get; set; }
        [ForeignKey(nameof(BshopId))]
        public Barbershop Bshop { get; set; }

        [Column("review_sum")]
        public int ReviewSum { get; set; } = 0;
        [Column("review_count")]
        public int ReviewCount { get; set; } = 0;

        [Column("fk_account")]
        public int? AccountId { get; set; }

        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; }

        // Navigation
        public ICollection<BarberJob> BarberJobs { get; set; }
        public ICollection<BarberScheduleDay> ScheduleDays { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<ImageBarber> Images { get; set; }
    }
}