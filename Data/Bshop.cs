namespace barberchainAPI.Data
{
    public class Bshop
    {
        public int Id { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public string Address { get; set; }

        public string Phone { get; set; }

        public int ReviewSum { get; set; }
        public int ReviewCount { get; set; }

        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }

        public int FkManagerAccount { get; set; }

        public Account Manager { get; set; }
    }
}
