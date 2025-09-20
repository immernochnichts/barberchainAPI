namespace barberchainAPI.Data
{
    public class Review
    {
        public int Id { get; set; }

        public int AccountId { get; set; }  // fk_account

        public int BarberId { get; set; }   // fk_barber

        public string? Text { get; set; }

        public short? Score { get; set; }   // Score between 0 and 5
    }
}
