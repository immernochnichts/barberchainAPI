using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourApp.Models
{
    public class OrderJob
    {
        [Required]
        public int OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; }

        [Required]
        public int JobId { get; set; }

        [ForeignKey(nameof(JobId))]
        public Job Job { get; set; }
    }
}