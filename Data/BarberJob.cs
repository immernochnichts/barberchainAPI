using System.Threading;

namespace barberchainAPI.Data
{
    public class BarberJob
    {
        public int FkBarber { get; set; }
        public int FkJob { get; set; }

        public short Price { get; set; }

        public Barber Barber { get; set; }
        public Job Job { get; set; }
    }
}
