using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace barberchainAPI.Data
{
    public class Complaint
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("fk_account_sender")]
        public int AccountSenderId { get; set; }
        [ForeignKey(nameof(AccountSenderId))]
        public Account Sender { get; set; }

        [Required]
        [Column("fk_account_target")]
        public int AccountTargetId { get; set; }
        [ForeignKey(nameof(AccountTargetId))]
        public Account Target { get; set; }

        [Required]
        [Column("message")]
        public string Message { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}