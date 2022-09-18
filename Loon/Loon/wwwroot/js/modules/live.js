export var name = "live";

var _map, _marker;

export function initMap(subscriptionKey) {
    _map = new atlas.Map('map-area', {
        center: [-114, 51],
        style: 'grayscale_dark',
        zoom: 12,
        language: 'en-US',
        authOptions: {
            authType: 'subscriptionKey',
            subscriptionKey: subscriptionKey
        }
    });


    _map.events.add('ready', function () {
        //Create a HTML marker and add it to the map.
        _marker = new atlas.HtmlMarker({
            htmlContent: '<div class="plane-container"><img src=\"/img/plane_blue.png\"/><div class="pulseIcon"></div></div>',
            position: [-114, 51]
        });

        _map.markers.add(_marker);

    });
}

export function updatePlanePos(latitude, longitude, trueTrack) {
    //atlas.animations.setCoordinates(_marker, [longitude, latitude], { duration: 250, autoPlay: true });

    _marker.setOptions({
        htmlContent: '<div class="plane-container"><img src="/img/plane_blue.png" style=\"transform: rotate(' + trueTrack + 'deg);\"/><div class="pulseIcon"></div></div>',
        position: [longitude, latitude]
    });
}