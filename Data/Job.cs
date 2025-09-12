namespace barberchainAPI.Data
{
    public class Job
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Descr { get; set; }

        // Duration in atomic time units (15 minutes)
        public short DurationAtu { get; set; }

        public short DefaultPrice { get; set; }

        public ICollection<BarberJob> BarberJobs { get; set; }

        public ICollection<OrderJob> OrderJobs { get; set; }
    }
}
