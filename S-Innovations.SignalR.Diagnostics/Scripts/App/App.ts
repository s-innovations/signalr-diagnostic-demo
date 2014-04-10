/// <amd-dependency path="hubs" />
/// <amd-dependency path="jquery" />


import ko = require('knockout');


class App {
    private _server: HubProxy;
    perfcounters = ko.observableArray([]);
    isRunning = ko.observable(false);
    constructor() {

        var connection = $.hubConnection('/signalr');
        connection.logging = true;

        this._server = connection.createHubProxy('diagnosticHub');
        this._server.on('broadcastInformation', (data) => {

            this.perfcounters(data.Perfs);

        });
        this._server.on('stopedMonitoring', () => {
            console.log('stopedMonitoring');
            //  toastr.info("Server stopped monitoring performance counters", "Server");
            this.isRunning(false);
        });
        this._server.on('startedMonitoring', () => {
            //    toastr.info("Server started monitoring performance counters", "Server");
            this.isRunning(true);
            // this.showgraph(true);
        });

        connection.reconnected(() => {
            console.log('reconnected');
        });
        connection.stateChanged((change) => {

            if (change.newState === $.signalR.connectionState.reconnecting) {
                console.log('Re-connecting');
            }
            else if (change.newState === $.signalR.connectionState.connected) {
                console.log('The server is online');

            }
            else if (change.newState === $.signalR.connectionState.connecting) {
                console.log('The server is trying to come online');
            }
            else if (change.newState === $.signalR.connectionState.disconnected) {
                console.log('The server is disconected');
            }

        });

        connection.start().fail(() => {
            //  toastr.error("Signalr Failed to start", "SignalR");
        });
    }


    startMonitor() {
        this._server.invoke('StartMonitor');
    }

    stopMonitor() {
        this._server.invoke('StopMonitor');
    }
    toggleState() {
        if (this.isRunning())
            this.stopMonitor();
        else {
            this.startMonitor();
        }


    }
}

export = App;