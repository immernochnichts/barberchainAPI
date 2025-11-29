using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace barberchainAPI.Data
{
    public enum OrderStatus
    {
        Pending,
        Accepted,
        Waiting,
        Processing,
        Complete,
        Declined
    }

    public enum OrderMethod
    {
        Online,
        Phone
    }

    public class Order
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("fk_account")]
        public int? AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; }

        [Required]
        [Column("fk_barber")]
        public int BarberId { get; set; }
        [ForeignKey(nameof(BarberId))]
        public Barber Barber { get; set; }

        [Required]
        [Column("order_time")]
        public DateTime OrderTime { get; set; }

        [Required]
        [Column("status")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        [Column("appointed_time")]
        public DateTime AppointedTime { get; set; }

        [Required]
        [Column("method")]
        public OrderMethod Method { get; set; } = OrderMethod.Online;

        [Required]
        [Column("is_paid")]
        public bool IsPaid { get; set; } = false;

        [Column("pending_until")]
        public DateTime? PendingUntil { get; set; }

        [MaxLength(15)]
        [Column("phone")]
        public string? Phone { get; set; }
        public ICollection<OrderJob> OrderJobs { get; set; }
    }
}