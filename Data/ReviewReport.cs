using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace barberchainAPI.Data
{
    public class ReviewReport
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("review_id")]
        public int ReviewId { get; set; }

        [ForeignKey(nameof(ReviewId))]
        public Review Review { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        [Column("reporter_id")]
        public string ReporterId { get; set; } = null!;  // UserId or guestId

        [Required]
        [Column("reported_at")]
        public DateTime ReportedAt { get; set; } = DateTime.Now;
    }
}
