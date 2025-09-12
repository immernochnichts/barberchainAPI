namespace barberchainAPI.Data
{
    public enum AccountRole
    {
        User,
        Barber,
        Manager,
        Admin
    }

    public class Account
    {
        public int Id { get; set; }

        public string Lastname { get; set; }
        public string Restname { get; set; }

        public string Bio { get; set; }

        public DateTime BirthDate { get; set; }

        public string? Email { get; set; }

        public byte[] Hash { get; set; }

        public DateTime RegTime { get; set; }

        public byte[]? ProfilePic { get; set; }
        public byte[]? ProfilePicPreview { get; set; }

        public AccountRole Role { get; set; }
    }
}
