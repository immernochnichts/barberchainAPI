using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourApp.Models
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
        public int Id { get; set; }

        [Required]
        public int BarberId { get; set; }
        [ForeignKey(nameof(BarberId))]
        public Barber Barber { get; set; }

        [Required]
        public DateTime RequestDate { get; set; }

        public string Message { get; set; }

        [Required]
        public ScheduleRequestStatus Status { get; set; } = ScheduleRequestStatus.Pending;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}