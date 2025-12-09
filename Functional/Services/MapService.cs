using Microsoft.JSInterop;

namespace barberchainAPI.Functional.Services
{
    public class MapService : IMapService
    {
        private readonly IJSRuntime _js;

        public MapService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task InitMapAsync(string elementId, double lat, double lng, int zoom)
        {
            await _js.InvokeVoidAsync("mapService.initMap", elementId, lat, lng, zoom);
        }

        public async Task AddMarkerAsync(string id, double lat, double lng, string? popup = null)
        {
            await _js.InvokeVoidAsync("mapService.addMarker", id, lat, lng, popup);
        }

        public async Task UpdateMarkerAsync(string id, double lat, double lng)
        {
            await _js.InvokeVoidAsync("mapService.updateMarker", id, lat, lng);
        }

        public async Task RemoveMarkerAsync(string id)
        {
            await _js.InvokeVoidAsync("mapService.removeMarker", id);
        }
    }
}
