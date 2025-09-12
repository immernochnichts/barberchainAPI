using System.Threading;

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

    public class Order
    {
        public int Id { get; set; }

        public int? FkAccount { get; set; }      // Formerly client
        public int? FkBarber { get; set; }

        public DateTime OrderTime { get; set; }

        public DateTime AppointedTime { get; set; }

        public OrderStatus Status { get; set; }

        public Account? Account { get; set; }

        public Barber? Barber { get; set; }

        public ICollection<OrderJob> OrderJobs { get; set; }
    }
}
