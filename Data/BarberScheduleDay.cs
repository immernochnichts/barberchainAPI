using System;
using System.Collections;
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
        public DateOnly Date { get; set; }

        [Column("atu_pattern")]
        public BitArray AtuPattern { get; set; }
    }
}