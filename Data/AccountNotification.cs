using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace barberchainAPI.Data
{
    public class AccountNotification
    {
        [Key, Column("fk_account", Order = 0)]
        public int AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; }

        [Key, Column("fk_notification", Order = 1)]
        public int NotificationId { get; set; }
        [ForeignKey(nameof(NotificationId))]
        public Notification Notification { get; set; }
    }
}
