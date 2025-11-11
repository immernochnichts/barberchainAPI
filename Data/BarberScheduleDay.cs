using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace barberchainAPI.Data
{
    public class BarberScheduleDay
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("fk_barber")]
        public int BarberId { get; set; }
        [ForeignKey(nameof(BarberId))]
        public Barber Barber { get; set; }

        [Required]
        [Column("date")]
        public DateTime Date { get; set; }

        [Column("atu_pattern")]
        public byte[] AtuPattern { get; set; }

        [Column("fk_manager_account")]
        public int? ManagerAccountId { get; set; }
        [ForeignKey(nameof(ManagerAccountId))]
        public Account Manager { get; set; }
    }
}