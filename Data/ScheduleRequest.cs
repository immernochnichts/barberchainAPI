using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace barberchainAPI.Data
{
    public enum ScheduleRequestStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class ScheduleRequest
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
        [Column("request_date")]
        public DateOnly RequestDate { get; set; }

        [Column("message")]
        public string Message { get; set; }

        [Required]
        [Column("status")]
        public ScheduleRequestStatus Status { get; set; } = ScheduleRequestStatus.Pending;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        [Column("atu_pattern")]
        public BitArray AtuPattern { get; set; }

        [Column("order_ids_to_decline")]
        public string? OrderIdsToDecline { get; set; }

        [Column("reason_rejected")]
        public string? ReasonRejected { get; set; }

        [Column("changes")]
        public string? Changes { get; set; }
    }
}