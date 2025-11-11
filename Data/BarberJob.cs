using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace barberchainAPI.Data
{
    public class BarberJob
    {
        [Key, Column("fk_barber", Order = 0)]
        public int BarberId { get; set; }
        [ForeignKey(nameof(BarberId))]
        public Barber Barber { get; set; }

        [Key, Column("fk_job", Order = 1)]
        public int JobId { get; set; }
        [ForeignKey(nameof(JobId))]
        public Job Job { get; set; }

        [Column("price")]
        public double? Price { get; set; }
    }
}