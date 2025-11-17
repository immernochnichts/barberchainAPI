using barberchainAPI.Data;

namespace barberchainAPI.Functional
{
    public class CartService
    {
        private const string CartKey = "cart";
        private readonly LocalStorageService _localStorage;

        public CartService(LocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<Cart> GetCart()
        {
            return await _localStorage.LoadAsync<Cart>(CartKey) ?? new Cart();
        }

        public async Task AddToCart(Job item)
        {
            if (item is null)
            {
                return;
            }

            var cart = await GetCart();
            cart.JobSet.Add(item);
            
            await _localStorage.SaveAsync(CartKey, cart);
        }

        public async Task RemoveItem(int jobId)
        {
            var cart = await GetCart();
            cart.JobSet.RemoveWhere(i => i.Id == jobId);
            await _localStorage.SaveAsync(CartKey, cart);
        }

        public async Task Clear()
        {
            await _localStorage.RemoveAsync(CartKey);
        }
    }
}
