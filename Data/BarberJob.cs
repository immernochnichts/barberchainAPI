using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace barberchainAPI.Data
{
    public class BarberJob
    {
        [Key, Column(Order = 0)]
        public int BarberId { get; set; }
        [ForeignKey(nameof(BarberId))]
        public Barber Barber { get; set; }

        [Key, Column(Order = 1)]
        public int JobId { get; set; }
        [ForeignKey(nameof(JobId))]
        public Job Job { get; set; }

        public double Price { get; set; }
    }
}