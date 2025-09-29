using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YourApp.Models
{
    public enum AdminActionType
    {
        PromoteToManager,
        FireManager,
        FireBarber,
        AddBarbershop
    }

    public class AdminActionLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AdminAccountId { get; set; }
        [ForeignKey(nameof(AdminAccountId))]
        public Account Admin { get; set; }

        public int? TargetAccountId { get; set; }
        [ForeignKey(nameof(TargetAccountId))]
        public Account Target { get; set; }

        [Required]
        public AdminActionType ActionType { get; set; }

        public string Details { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}