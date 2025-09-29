using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourApp.Models
{
    public class BarberScheduleDay
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BarberId { get; set; }
        [ForeignKey(nameof(BarberId))]
        public Barber Barber { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public byte[] AtuPattern { get; set; }

        public int? ManagerAccountId { get; set; }
        [ForeignKey(nameof(ManagerAccountId))]
        public Account Manager { get; set; }
    }
}