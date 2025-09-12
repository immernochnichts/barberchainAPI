namespace barberchainAPI.Data
{
    public class BshopImage
    {
        public int Id { get; set; }

        public int FkBshop { get; set; }

        public byte[] ImgFile { get; set; }

        public Bshop Bshop { get; set; }
    }
}
