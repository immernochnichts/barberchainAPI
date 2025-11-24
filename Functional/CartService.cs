using barberchainAPI.Data;
using barberchainAPI.Functional;
using System.Security.Claims;

public class CartService
{
    private const string BaseCartKey = "cart";
    private readonly LocalStorageService _localStorage;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(LocalStorageService localStorage, IHttpContextAccessor httpContextAccessor)
    {
        _localStorage = localStorage;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetCartKey()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
                return $"{BaseCartKey}_{userId}";
        }

        return BaseCartKey + "_anon";
    }

    public async Task<Cart> GetCart()
    {
        var key = GetCartKey();
        return await _localStorage.LoadAsync<Cart>(key) ?? new Cart();
    }

    public async Task AddToCart(Job item)
    {
        if (item is null) return;

        var cart = await GetCart();
        cart.JobSet.Add(item);

        var key = GetCartKey();
        await _localStorage.SaveAsync(key, cart);
    }

    public async Task RemoveItem(int jobId)
    {
        var cart = await GetCart();
        cart.JobSet.RemoveWhere(i => i.Id == jobId);

        var key = GetCartKey();
        await _localStorage.SaveAsync(key, cart);
    }

    public async Task Clear()
    {
        var key = GetCartKey();
        await _localStorage.RemoveAsync(key);
    }
}
