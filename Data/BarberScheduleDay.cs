using System.Collections;

namespace barberchainAPI.Data
{
    public class BarberScheduleDay
    {
        public int Id { get; set; }

        public int FkBarber { get; set; }
        public DateOnly Date { get; set; }

        public BitArray AtuPattern { get; set; }

        public int? FkManagerAccount { get; set; }
        public Account? CreatedByManager { get; set; }
    }
}
