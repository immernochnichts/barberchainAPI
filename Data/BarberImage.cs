namespace barberchainAPI.Data
{
    public class BarberImage
    {
        public int Id { get; set; }

        public int FkBarber { get; set; }

        public byte[] ImgFile { get; set; }

        public Barber Barber { get; set; }
    }
}
