// Google Maps JS interop for Authority.Records Location Module
// This module is loaded on-demand when the Location Create/Edit pages initialise the map picker.

'use strict';

let _mapsLoaded = false;
let _mapsLoadPromise = null;
const _mapInstances = {};   // elementId → { map, marker, autocomplete }

/**
 * Dynamically load the Google Maps JS API once per page lifetime.
 * Subsequent calls return the same promise so the script is never injected twice.
 * @param {string} apiKey
 * @returns {Promise<void>}
 */
function loadGoogleMapsApi(apiKey) {
    if (_mapsLoaded) return Promise.resolve();
    if (_mapsLoadPromise) return _mapsLoadPromise;

    _mapsLoadPromise = new Promise((resolve, reject) => {
        const callbackName = '__googleMapsReady_' + Date.now();
        window[callbackName] = () => {
            _mapsLoaded = true;
            delete window[callbackName];
            resolve();
        };
        const script = document.createElement('script');
        script.src = `https://maps.googleapis.com/maps/api/js?key=${encodeURIComponent(apiKey)}&libraries=places&callback=${callbackName}&loading=async`;
        script.async = true;
        script.onerror = () => reject(new Error('Failed to load Google Maps API'));
        document.head.appendChild(script);
    });

    return _mapsLoadPromise;
}

/**
 * Initialise an interactive map picker inside the element with the given id.
 * @param {string} elementId        - id of the <div> to render the map into
 * @param {number} lat              - initial centre latitude
 * @param {number} lng              - initial centre longitude
 * @param {string} searchInputId    - id of the <input> to attach Places Autocomplete to
 * @param {object} dotnetRef        - DotNetObjectReference for callbacks
 */
async function initLocationPicker(elementId, lat, lng, searchInputId, dotnetRef) {
    const container = document.getElementById(elementId);
    if (!container) return;

    const center = { lat, lng };

    const map = new google.maps.Map(container, {
        center,
        zoom: 14,
        mapTypeControl: false,
        streetViewControl: false,
        fullscreenControl: false
    });

    const marker = new google.maps.Marker({
        map,
        position: center,
        draggable: true
    });

    // Reverse geocode on marker drag end
    marker.addListener('dragend', async () => {
        const pos = marker.getPosition();
        await reverseGeocode(pos.lat(), pos.lng(), dotnetRef);
    });

    // Click on map moves marker
    map.addListener('click', async (e) => {
        marker.setPosition(e.latLng);
        await reverseGeocode(e.latLng.lat(), e.latLng.lng(), dotnetRef);
    });

    // Places Autocomplete
    const input = document.getElementById(searchInputId);
    if (input) {
        const autocomplete = new google.maps.places.Autocomplete(input, {
            fields: ['address_components', 'formatted_address', 'geometry', 'name'],
            types: ['address']
        });

        autocomplete.addListener('place_changed', async () => {
            const place = autocomplete.getPlace();
            if (!place.geometry) return;

            const loc = place.geometry.location;
            map.panTo(loc);
            map.setZoom(17);
            marker.setPosition(loc);

            await notifyPlaceSelected(place.address_components, loc.lat(), loc.lng(), dotnetRef, place.formatted_address);
        });

        _mapInstances[elementId] = { map, marker, autocomplete };
    } else {
        _mapInstances[elementId] = { map, marker };
    }
}

/**
 * Initialise a read-only static map centred on the given coordinates.
 * @param {string} elementId
 * @param {number} lat
 * @param {number} lng
 */
async function initStaticMap(elementId, lat, lng) {
    const container = document.getElementById(elementId);
    if (!container) return;

    new google.maps.Map(container, {
        center: { lat, lng },
        zoom: 16,
        mapTypeControl: false,
        streetViewControl: false,
        fullscreenControl: false,
        draggable: false,
        zoomControl: true,
        scrollwheel: false,
        disableDoubleClickZoom: true
    });

    new google.maps.Marker({ map: _mapInstances[elementId]?.map, position: { lat, lng } });
}

/**
 * Move the map and marker to new coordinates (e.g. when the user edits coordinates manually).
 * @param {string} elementId
 * @param {number} lat
 * @param {number} lng
 */
function setMapCenter(elementId, lat, lng) {
    const instance = _mapInstances[elementId];
    if (!instance) return;
    const pos = { lat, lng };
    instance.map.panTo(pos);
    instance.marker.setPosition(pos);
}

/**
 * Tear down a map instance and release resources.
 * @param {string} elementId
 */
function destroyMap(elementId) {
    const instance = _mapInstances[elementId];
    if (!instance) return;
    if (instance.autocomplete) google.maps.event.clearInstanceListeners(instance.autocomplete);
    google.maps.event.clearInstanceListeners(instance.map);
    google.maps.event.clearInstanceListeners(instance.marker);
    instance.marker.setMap(null);
    delete _mapInstances[elementId];
}

// ─── Internal helpers ─────────────────────────────────────────────────────────

async function reverseGeocode(lat, lng, dotnetRef) {
    const geocoder = new google.maps.Geocoder();
    const result = await geocoder.geocode({ location: { lat, lng } });
    if (result.results && result.results.length > 0) {
        await notifyPlaceSelected(result.results[0].address_components, lat, lng, dotnetRef,
            result.results[0].formatted_address);
    }
}

async function notifyPlaceSelected(components, lat, lng, dotnetRef, formattedAddress) {
    const get = (type, useShort) => {
        const c = components.find(c => c.types.includes(type));
        return c ? (useShort ? c.short_name : c.long_name) : null;
    };

    const payload = {
        streetNumber:     get('street_number', false),
        route:            get('route', false),
        aptSuite:         get('subpremise', false),
        city:             get('locality', false) ?? get('sublocality', false) ?? get('administrative_area_level_2', false),
        state:            get('administrative_area_level_1', true),   // short name, e.g. "IL"
        country:          get('country', true),                       // short name, e.g. "US"
        zip:              get('postal_code', false),
        lat,
        lng,
        formattedAddress: formattedAddress ?? null
    };

    try {
        await dotnetRef.invokeMethodAsync('OnPlaceSelected', payload);
    } catch (e) {
        console.warn('[googleMaps] OnPlaceSelected callback failed:', e);
    }
}

// Export public API
export { loadGoogleMapsApi, initLocationPicker, initStaticMap, setMapCenter, destroyMap };
