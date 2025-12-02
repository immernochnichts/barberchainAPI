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
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("fk_actor_account")]
        public int ActorAccountId { get; set; }
        [ForeignKey(nameof(ActorAccountId))]
        public Account ActorAccount { get; set; }

        [Column("fk_target_account")]
        public int? TargetAccountId { get; set; }
        [ForeignKey(nameof(TargetAccountId))]
        public Account TargetAccount { get; set; }

        [Required]
        [Column("type")]
        public ActionType ActionType { get; set; }

        [Column("details")]
        public string? Details { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}