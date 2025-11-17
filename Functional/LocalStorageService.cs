using Microsoft.JSInterop;
using System.Text.Json;

namespace barberchainAPI.Functional
{
    public class LocalStorageService
    {
        private readonly IJSRuntime _js;

        public LocalStorageService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task SaveAsync<T>(string key, T data)
        {
            var json = JsonSerializer.Serialize(data);
            await _js.InvokeVoidAsync("localStorageHelper.setItem", key, json);
        }

        public async Task<T?> LoadAsync<T>(string key)
        {
            var json = await _js.InvokeAsync<string>("localStorageHelper.getItem", key);
            return json == null ? default : JsonSerializer.Deserialize<T>(json);
        }

        public async Task RemoveAsync(string key)
        {
            await _js.InvokeVoidAsync("localStorageHelper.removeItem", key);
        }
    }
}
