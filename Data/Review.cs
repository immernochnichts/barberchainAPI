using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace barberchainAPI.Data
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; }

        [Required]
        public int BarberId { get; set; }
        [ForeignKey(nameof(BarberId))]
        public Barber Barber { get; set; }

        public string Text { get; set; }

        public short? Score { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}