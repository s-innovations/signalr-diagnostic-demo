require.config({
    // Once you setup baseUrl
    // Relative urls continue to work normal (from source file).
    // However Non-relative URLs use this as base.
    // By default this is the location of requirejs.
    baseUrl: 'Scripts/',
    shim: {
        "signalr": {
            deps: ["jquery"]
        },
        "hubs": {
            deps: ["signalr"]
        }
    },
    paths: {
        "knockout": 'knockout-3.1.0',
        "jquery": 'jquery-1.6.4.min',
        "signalr": "jquery.signalR-2.0.3.min",
        "hubs": "/signalr/hubs?"
    }
});

require(['knockout', 'App/App'], function (ko, App) {
    var uploader = null;
    var app = new App();
    ko.applyBindings(app);
});
//# sourceMappingURL=main.js.map
