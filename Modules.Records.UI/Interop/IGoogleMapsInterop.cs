using Microsoft.JSInterop;

namespace Modules.Records.UI.Interop;

/// <summary>JS interop contract for Google Maps picker and static map display.</summary>
public interface IGoogleMapsInterop : IAsyncDisposable
{
    /// <summary>Dynamically loads the Google Maps API script into the page (idempotent).</summary>
    Task LoadApiAsync(string apiKey);

    /// <summary>Initialises an interactive map picker inside <paramref name="container"/>.</summary>
    /// <param name="container">The element reference for the map div.</param>
    /// <param name="elementId">HTML id of the map div (needed for destroy).</param>
    /// <param name="searchInputId">HTML id of the Places Autocomplete input.</param>
    /// <param name="lat">Initial center latitude.</param>
    /// <param name="lng">Initial center longitude.</param>
    /// <param name="dotnetRef">DotNetObjectReference (any type) whose <c>OnPlaceSelected</c> method receives callbacks.</param>
    Task InitPickerAsync(string elementId, string searchInputId, double lat, double lng, object dotnetRef);

    /// <summary>Renders a read-only map centred on the given coordinates.</summary>
    Task InitStaticMapAsync(string elementId, double lat, double lng);

    /// <summary>Pans the map and moves the marker to new coordinates.</summary>
    Task SetCenterAsync(string elementId, double lat, double lng);

    /// <summary>Tears down the map instance and releases event listeners.</summary>
    Task DestroyAsync(string elementId);
}
