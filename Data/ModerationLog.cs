using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace barberchainAPI.Data
{
    public class ModerationLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReviewId { get; set; }
        [ForeignKey(nameof(ReviewId))]
        public Review Review { get; set; }

        [Required]
        public int AccountManagerId { get; set; }
        [ForeignKey(nameof(AccountManagerId))]
        public Account Manager { get; set; }

        public string Reason { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}