namespace barberchainAPI.Data
{
    public class Barber
    {
        public int Id { get; set; }

        public int FkAccount { get; set; }

        public int? FkBshop { get; set; }

        public int ReviewSum { get; set; }

        public int ReviewCount { get; set; }

        public Account Account { get; set; }

        public Bshop? Bshop { get; set; }

        public ICollection<BarberJob> BarberJobs { get; set; }

        public ICollection<Order> Orders { get; set; }

        public ICollection<BarberImage> Images { get; set; }

        public ICollection<BarberScheduleDay> ScheduleDays { get; set; }
    }
}
