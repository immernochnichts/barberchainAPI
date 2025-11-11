using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace barberchainAPI.Data
{
    public enum ActionType
    {
        HireBarber,
        PromoteToManager,
        FireManager,
        FireBarber,
        AddBarbershop
    }

    public class ActionLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ActorAccountId { get; set; }
        [ForeignKey(nameof(ActorAccountId))]
        public Account ActorAccount { get; set; }

        public int? TargetAccountId { get; set; }
        [ForeignKey(nameof(TargetAccountId))]
        public Account TargetAccount { get; set; }

        [Required]
        public ActionType ActionType { get; set; }

        public string Details { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}