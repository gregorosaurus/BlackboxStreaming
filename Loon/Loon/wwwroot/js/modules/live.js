export var name = "live";

var _map, _marker;

export function initMap(subscriptionKey) {
    _map = new atlas.Map('map-area', {
        center: [-114.3741, 51.1028],
        style: 'grayscale_dark',
        zoom: 14,
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
            position: [-114.3741, 51.1028]
        });

        _map.markers.add(_marker);

    });
}

export function updatePlanePos(latitude, longitude, trueTrack) {
    atlas.animations.setCoordinates(_marker, [longitude, latitude], { duration: 250, autoPlay: true });

    _marker.setOptions({
        htmlContent: '<div class="plane-container"><img src="/img/plane_blue.png" style=\"transform: rotate(' + trueTrack + 'deg);\"/><div class="pulseIcon"></div></div>',
    });

}

var attitudeInstrument;
var headingInstrument;
var variometerInstrument;
var airspeedInstrument;
var altimeterInstrument;
var turn_coordinatorInstrument;

var _altitude = 0
var _speed = 0;
var _heading = 0;

var instrAltitude = 0;
var instrSpeed = 0;
var instrHeading = 0;

export function initGauges() {
    attitudeInstrument = $.flightIndicator('#attitude', 'attitude', { roll: 50, pitch: -20, size: 300, showBox: true });
    headingInstrument = $.flightIndicator('#heading', 'heading', { heading: 150, size: 300,showBox: true });
    variometerInstrument = $.flightIndicator('#variometer', 'variometer', { vario: -5, size: 300, showBox: true });
    airspeedInstrument = $.flightIndicator('#airspeed', 'airspeed', { size: 300, showBox: true });
    altimeterInstrument = $.flightIndicator('#altimeter', 'altimeter', { size: 300, showBox: true });
    //turn_coordinator = $.flightIndicator('#turn_coordinator', 'turn_coordinator', { turn: 0 });


    setInterval(function () {
        //we have to animate to the current value.

        var altitudeDiff = _altitude - instrAltitude;
        var altitudeChange = altitudeDiff / 50.0;
        instrAltitude = altitudeChange + instrAltitude;
        altimeterInstrument.setAltitude(instrAltitude);

        var speedDiff = _speed - instrSpeed;
        var speedChange = speedDiff / 50.0;
        instrSpeed = speedChange + instrSpeed;
        airspeedInstrument.setAirSpeed(instrSpeed);

        var headingDiff = _heading - instrHeading;
        var headingChange = headingDiff / 50.0
        instrHeading = headingChange + instrHeading
        headingInstrument.setHeading(instrHeading);

    }, 50);
}

export function updateGauges(speed, altitude, trackAngle, verticalSpeed) {
    _altitude = altitude;
    _heading = trackAngle;
    _speed = speed;
}