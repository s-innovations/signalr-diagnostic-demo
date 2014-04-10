/// <amd-dependency path="hubs" />
/// <amd-dependency path="jquery" />
define(["require", "exports", 'knockout', "hubs", "jquery"], function(require, exports, ko) {
    var App = (function () {
        function App() {
            var _this = this;
            this.perfcounters = ko.observableArray([]);
            this.isRunning = ko.observable(false);
            var connection = $.hubConnection('/signalr');
            connection.logging = true;

            this._server = connection.createHubProxy('diagnosticHub');
            this._server.on('broadcastInformation', function (data) {
                _this.perfcounters(data.Perfs);
            });
            this._server.on('stopedMonitoring', function () {
                console.log('stopedMonitoring');

                //  toastr.info("Server stopped monitoring performance counters", "Server");
                _this.isRunning(false);
            });
            this._server.on('startedMonitoring', function () {
                //    toastr.info("Server started monitoring performance counters", "Server");
                _this.isRunning(true);
                // this.showgraph(true);
            });

            connection.reconnected(function () {
                console.log('reconnected');
            });
            connection.stateChanged(function (change) {
                if (change.newState === $.signalR.connectionState.reconnecting) {
                    console.log('Re-connecting');
                } else if (change.newState === $.signalR.connectionState.connected) {
                    console.log('The server is online');
                } else if (change.newState === $.signalR.connectionState.connecting) {
                    console.log('The server is trying to come online');
                } else if (change.newState === $.signalR.connectionState.disconnected) {
                    console.log('The server is disconected');
                }
            });

            connection.start().fail(function () {
                //  toastr.error("Signalr Failed to start", "SignalR");
            });
        }
        App.prototype.startMonitor = function () {
            this._server.invoke('StartMonitor');
        };

        App.prototype.stopMonitor = function () {
            this._server.invoke('StopMonitor');
        };
        App.prototype.toggleState = function () {
            if (this.isRunning())
                this.stopMonitor();
            else {
                this.startMonitor();
            }
        };
        return App;
    })();

    
    return App;
});
//# sourceMappingURL=App.js.map
