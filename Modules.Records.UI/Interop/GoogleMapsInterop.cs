using Microsoft.JSInterop;

namespace Modules.Records.UI.Interop;

public sealed class GoogleMapsInterop : IGoogleMapsInterop
{
    private readonly IJSRuntime _js;
    private IJSObjectReference? _module;

    public GoogleMapsInterop(IJSRuntime js)
    {
        _js = js;
    }

    private async Task<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await _js.InvokeAsync<IJSObjectReference>("import", "/js/googleMaps.js");
        return _module;
    }

    public async Task LoadApiAsync(string apiKey)
    {
        var mod = await GetModuleAsync();
        await mod.InvokeVoidAsync("loadGoogleMapsApi", apiKey);
    }

    public async Task InitPickerAsync(
        string elementId,
        string searchInputId,
        double lat,
        double lng,
        object dotnetRef)
    {
        var mod = await GetModuleAsync();
        await mod.InvokeVoidAsync("initLocationPicker", elementId, lat, lng, searchInputId, dotnetRef);
    }

    public async Task InitStaticMapAsync(string elementId, double lat, double lng)
    {
        var mod = await GetModuleAsync();
        await mod.InvokeVoidAsync("initStaticMap", elementId, lat, lng);
    }

    public async Task SetCenterAsync(string elementId, double lat, double lng)
    {
        var mod = await GetModuleAsync();
        await mod.InvokeVoidAsync("setMapCenter", elementId, lat, lng);
    }

    public async Task DestroyAsync(string elementId)
    {
        if (_module is null) return;
        await _module.InvokeVoidAsync("destroyMap", elementId);
    }

    public async Task InitMarkersMapAsync(string elementId, object markers, double defaultLat, double defaultLng)
    {
        var mod = await GetModuleAsync();
        await mod.InvokeVoidAsync("initMarkersMap", elementId, markers, defaultLat, defaultLng);
    }

    public async Task ToggleMarkerLayerAsync(string elementId, string recordType, bool visible)
    {
        if (_module is null) return;
        await _module.InvokeVoidAsync("toggleMarkerLayer", elementId, recordType, visible);
    }

    public async Task TogglePoiLayerAsync(string elementId, bool visible)
    {
        if (_module is null) return;
        await _module.InvokeVoidAsync("togglePoiLayer", elementId, visible);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is null) return;
        try
        {
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException) { }
        catch (JSException) { }
        finally
        {
            _module = null;
        }
    }
}
