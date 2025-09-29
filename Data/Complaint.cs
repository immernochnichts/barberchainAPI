using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourApp.Models
{
    public enum ComplaintStatus
    {
        Pending,
        Resolved,
        Dismissed
    }

    public class Complaint
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AccountSenderId { get; set; }
        [ForeignKey(nameof(AccountSenderId))]
        public Account Sender { get; set; }

        [Required]
        public int AccountTargetId { get; set; }
        [ForeignKey(nameof(AccountTargetId))]
        public Account Target { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;
    }
}