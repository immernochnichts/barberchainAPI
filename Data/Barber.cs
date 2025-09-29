using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourApp.Models
{
    public class Barber
    {
        [Key]
        public int Id { get; set; }

        public int? BshopId { get; set; }
        [ForeignKey(nameof(BshopId))]
        public Barbershop Bshop { get; set; }

        public int ReviewSum { get; set; } = 0;
        public int ReviewCount { get; set; } = 0;

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