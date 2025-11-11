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
        public int Id { get; set; }

        public int? AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; }

        [Required]
        public int BarberId { get; set; }
        [ForeignKey(nameof(BarberId))]
        public Barber Barber { get; set; }

        [Required]
        public DateTime OrderTime { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        public DateTime AppointedTime { get; set; }

        [Required]
        public OrderMethod Method { get; set; } = OrderMethod.Online;

        public ICollection<OrderJob> OrderJobs { get; set; }
    }
}