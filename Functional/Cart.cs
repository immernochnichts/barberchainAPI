using barberchainAPI.Data;

namespace barberchainAPI.Functional
{
    public class Cart
    {
        public HashSet<Job> JobSet { get; set; } = new();
    }
}
