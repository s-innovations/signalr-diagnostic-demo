require.config({
    // Once you setup baseUrl
    // Relative urls continue to work normal (from source file).
    // However Non-relative URLs use this as base.
    // By default this is the location of requirejs.
    baseUrl: 'Scripts/',
    shim: {
        'sammy': {
            deps: ['jquery']
        },
        "bootstrap": {
            deps: ["jquery"]
        },
        "bootstrapDocs": {
            deps: ["bootstrap"]
        }
    },
    paths: {
        "knockout.mapping": 'knockout.mapping-latest',
        "knockout": 'knockout-3.1.0',
        "StorageAccountViewModel": 'app/StorageAccountViewModel',
        "bootstrapDocs": 'docs.min',
        "bootstrap": 'bootstrap',
        "sammy": "sammy-0.7.4.min",
        "jquery": 'jquery-2.1.0.min',
        "fileUploader": 'FileUpload/FileUploader'
    }
});

require(['knockout', 'App/App'], function (ko, App) {
    var uploader = null;
    var app = new App();
    ko.applyBindings(app);
});
//# sourceMappingURL=app.js.map
