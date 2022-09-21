export var name = "live";

var _map, _marker;

export function initMap(subscriptionKey) {
    _map = new atlas.Map('map-area', {
        center: [-114.36397546454964, 51.10416347244066],
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
            position: [-114.36397546454964, 51.10416347244066]
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
var _verticalSpeed = 0;
var _turnRate = 0;
var _pitch = 0;
var _roll = 0;

var instrAltitude = 0;
var instrSpeed = 0;
var instrHeading = 0;
var instrVerticalSpeed = 0;
var instrTurnRate = 0;
var instrPitch = 0;
var instrRoll = 0;


export function initGauges() {
    attitudeInstrument = $.flightIndicator('#attitude', 'attitude', { roll: 50, pitch: -20, size: 300, showBox: true });
    headingInstrument = $.flightIndicator('#heading', 'heading', { heading: 150, size: 300,showBox: true });
    variometerInstrument = $.flightIndicator('#variometer', 'variometer', { vario: -5, size: 300, showBox: true });
    airspeedInstrument = $.flightIndicator('#airspeed', 'airspeed', { size: 300, showBox: true });
    altimeterInstrument = $.flightIndicator('#altimeter', 'altimeter', { size: 300, showBox: true });
    turn_coordinatorInstrument = $.flightIndicator('#turn_coordinator', 'turn_coordinator', { turn: 0, size: 300, });


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

        var verticalSpeedDiff = _verticalSpeed - instrVerticalSpeed;
        var verticalSpeedChange = verticalSpeedDiff / 50.0;
        instrVerticalSpeed = verticalSpeedChange + instrVerticalSpeed;
        variometerInstrument.setVario(instrVerticalSpeed);

        var turnRateDiff = _turnRate - instrTurnRate;
        var turnRateChange = turnRateDiff / 50.0;
        instrTurnRate = turnRateChange + instrTurnRate;
        turn_coordinatorInstrument.setTurn(instrTurnRate);

        var rollDiff = _roll - instrRoll;
        var rollChange = rollDiff / 50.0;
        instrRoll = rollChange + instrRoll;
        attitudeInstrument.setRoll(instrRoll);

        var pitchDiff = _pitch - instrPitch;
        var pitchChange = pitchDiff / 50.0;
        instrPitch = pitchChange + instrPitch;
        attitudeInstrument.setPitch(instrPitch);

    }, 50);
}

export function updateGauges(speed, altitude, trackAngle, verticalSpeed, turnRate, rollAngle, pitchAngle) {
    _altitude = altitude;
    _heading = trackAngle;
    _speed = speed;
    _verticalSpeed = verticalSpeed;
    _turnRate = turnRate;
    _roll = -rollAngle; //roll is opposite on the gauges. 
    _pitch = pitchAngle * 1.5; /*make it bigger so it just looks bigger on the display*/

    updateAltitudeChart(altitude);
}

var altitudeChart;

export function initCharts() {

    var canvas = document.getElementById('altitudeChart');
    if (canvas == null)
        return;

    const ctx = canvas.getContext('2d');

    const data = {
        labels: [],
        datasets: [
            {
                label: 'Altitude',
                data: [],
                borderColor: '#0056a6',
                backgroundColor: '#0056a6'
            },
        ]
    };

    altitudeChart = new Chart(ctx, {
        type: 'line',
        data: data,
        options: {
            plugins: {
                legend: {
                    display: false,
                },
                title: {
                    display: false,
                }
            },
            scales: {
                x: {
                    display: false,
                },
                y: {
                    display: true,
                }
            },
            maintainAspectRatio: false,
            tension: 0.1,
            pointRadius: 0,
            borderWidth:5
        }
    });
}

export function updateAltitudeChart(altitude) {
    altitudeChart.data.labels.push("");
    altitudeChart.data.datasets.forEach((dataset) => {
        dataset.data.push(altitude);
    });
    altitudeChart.update();
}