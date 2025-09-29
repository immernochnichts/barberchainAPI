using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourApp.Models
{
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
        public string Status { get; set; } = "pending";

        [Required]
        public DateTime AppointedTime { get; set; }

        [Required]
        public OrderMethod Method { get; set; } = OrderMethod.Online;

        public ICollection<OrderJob> OrderJobs { get; set; }
    }
}