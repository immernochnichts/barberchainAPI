using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace barberchainAPI.Data
{
    public enum NotificationType
    {
        General,
        Warning,
        Promotion,
        System
    }

    public class Notification
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("notification_type")]
        public NotificationType Type { get; set; }

        [Required]
        [Column("content")]
        public string Content { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
