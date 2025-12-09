window.mapService = {
    map: null,
    markers: {},

    initMap: function (elementId, lat, lng, zoom) {
        this.map = L.map(elementId).setView([lat, lng], zoom);

        L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', {
            attribution: '&copy; CARTO'
        }).addTo(this.map);
    },

    addMarker: function (id, lat, lng, linkUrl) {
        const blackIcon = L.icon({
            iconUrl: '/images/marker-black.png',
            shadowUrl: '/images/marker-shadow.png',
            iconSize: [25, 41],
            iconAnchor: [12, 41],
            popupAnchor: [1, -34]
        });

        const marker = L.marker([lat, lng], { icon: blackIcon });

        if (linkUrl) {
            marker.bindPopup(`<a href="${linkUrl}">Перейти</a>`);
        }

        marker.addTo(this.map);
        this.markers[id] = marker;
    },

    updateMarker: function (id, lat, lng) {
        const marker = this.markers[id];
        if (marker) {
            marker.setLatLng([lat, lng]);
        }
    },

    removeMarker: function (id) {
        const marker = this.markers[id];
        if (marker) {
            this.map.removeLayer(marker);
            delete this.markers[id];
        }
    }
};
