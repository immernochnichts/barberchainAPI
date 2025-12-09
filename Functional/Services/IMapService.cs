namespace barberchainAPI.Functional.Services
{
    public interface IMapService
    {
        Task InitMapAsync(string elementId, double lat, double lng, int zoom);

        Task AddMarkerAsync(string id, double lat, double lng, string? popup = null);

        Task UpdateMarkerAsync(string id, double lat, double lng);

        Task RemoveMarkerAsync(string id);
    }
}
