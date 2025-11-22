using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace barberchainAPI.Data
{
    public class OrderJob
    {
        [Required]
        [Column("fk_order")]
        public int OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; }

        [Required]
        [Column("fk_job")]
        public int JobId { get; set; }

        [ForeignKey(nameof(JobId))]
        public Job Job { get; set; }
    }
}