namespace barberchainAPI.Data
{
    public class OrderJob
    {
        public int Id { get; set; }

        public int FkOrder { get; set; }

        public int FkJob { get; set; }

        public Order Order { get; set; }

        public Job Job { get; set; }
    }
}
