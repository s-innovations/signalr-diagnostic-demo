using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using SInnovations.SignalR.Diagnostics;

[assembly: OwinStartup(typeof(OwinStartup))]

namespace SInnovations.SignalR.Diagnostics
{
    public class OwinStartup
    {
        public void Configuration(IAppBuilder app)
        {

            app.MapSignalR();

            app.Map("/signalr-diagnostic", builder=>{

                builder.Run(ctx =>
                    ctx.Response.WriteAsync(
@"<!DOCTYPE html>

<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <title>TypeScript HTML App</title>
    <link rel=""stylesheet"" href=""app.css"" type=""text/css"" />
    <script data-main=""main"" src=""Scripts/require.js""></script>
</head>
<body>
    <h1>TypeScript HTML App</h1>
<button data-bind=""click:toggleState"">Toggle</button>
    <div data-bind=""foreach : perfcounters"">
        <span data-bind=""text:Name""></span> : <span data-bind=""text:Value""></span><br />
    </div>
</body>
</html>"));
          

            });
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
        }
    }

    public enum MonitorState
    {
        Stopped, Running
    }


    public class SignalRDiagnosticNotficationHub
    {
        private static readonly Lazy<SignalRDiagnosticNotficationHub> _instance = new Lazy<SignalRDiagnosticNotficationHub>(
            () => new SignalRDiagnosticNotficationHub(GlobalHost.ConnectionManager.GetHubContext<DiagnosticHub>().Clients));

        private readonly object _marketStateLock = new object();
        private readonly object _updateStockPricesLock = new object();


        private TimeSpan _updateInterval = TimeSpan.FromMilliseconds(500);

        public TimeSpan BroadcastInterval
        {
            set
            {
                _updateInterval = value;
                _timer.Change(value, value);
            }
        }


        private Timer _timer;
        private volatile bool _updatingStockPrices;
        private volatile MonitorState _marketState;


        private readonly Lazy<PerformanceCounter[]> _performanceCounters
            = new Lazy<PerformanceCounter[]>(perfist);

        private static PerformanceCounter[] perfist()
        {
            var dict = new List<PerformanceCounter>();
            var includes = new[] { "Processor,% Processor Time,_Total", "Memory,Available MBytes", "SignalR" };

            var categories =
                System.Diagnostics.PerformanceCounterCategory.GetCategories().Where(c => includes.Any(cat => cat.Split(',').First() == c.CategoryName));

            foreach (PerformanceCounterCategory category in categories)
            {
                var countername = includes.Where(s => s.Split(',').First() == category.CategoryName).Select(s => s.Split(',').Skip(1).FirstOrDefault());

                if (category.CategoryType != PerformanceCounterCategoryType.SingleInstance)
                {
                    var names = category.GetInstanceNames().Where(n => includes.Any(ins => (ins.Split(',').Skip(2).FirstOrDefault() ?? n) == n));
                    foreach (string name in names)
                    {


                        dict.AddRange(category.GetCounters(name).Where(
                            pc => (countername.FirstOrDefault() ?? pc.CounterName) == pc.CounterName));

                    }
                }
                else
                {
                    dict.AddRange(category.GetCounters().Where(
                            pc => (countername.FirstOrDefault() ?? pc.CounterName) == pc.CounterName));

                }
            }
            return dict.ToArray();
        }

        private SignalRDiagnosticNotficationHub(IHubConnectionContext clients)
        {


            Clients = clients;       


        }

     

        public static SignalRDiagnosticNotficationHub Instance { get { return _instance.Value; } }

        private IHubConnectionContext Clients
        {
            get;
            set;
        }

        public MonitorState MonitorState
        {
            get { return _marketState; }
            private set { _marketState = value; }
        }

        public void StartMonitor()
        {
            lock (_marketStateLock)
            {
                if (MonitorState != MonitorState.Running)
                {
                    _timer = new Timer(UpdateStockPrices, null, _updateInterval, _updateInterval);

                    MonitorState = MonitorState.Running;

                    BroadcastMarketStateChange(MonitorState.Running);
                }
            }
        }
        public void StopMonitor()
        {
            lock (_marketStateLock)
            {
                if (MonitorState == MonitorState.Running)
                {
                    if (_timer != null)
                    {
                        _timer.Dispose();
                    }

                    MonitorState = MonitorState.Stopped;

                    BroadcastMarketStateChange(MonitorState.Stopped);
                }
            }
        }
        private void UpdateStockPrices(object state)
        {
            // This function must be re-entrant as it's running as a timer interval handler
            lock (_updateStockPricesLock)
            {
                if (!_updatingStockPrices)
                {
                    _updatingStockPrices = true;


                   
                    Clients.All.BroadcastInformation(new
                    {                       
                        Perfs = _performanceCounters.Value.Select(p =>
                            new
                            {
                                Name = p.CounterName,
                                Value = p.NextValue().ToString("0.0")
                            })
                    });

                    _updatingStockPrices = false;
                }
            }
        }

        private void BroadcastMarketStateChange(MonitorState marketState)
        {
            switch (marketState)
            {
                case MonitorState.Running:
                    Clients.All.StartedMonitoring();
                    break;
                case MonitorState.Stopped:
                    Clients.All.StopedMonitoring();
                    break;
                default:
                    break;
            }
        }


    }

    public class DiagnosticHub : Hub
    {
        private readonly SignalRDiagnosticNotficationHub _stockTicker;

        public DiagnosticHub() : this(SignalRDiagnosticNotficationHub.Instance) { }

        public DiagnosticHub(SignalRDiagnosticNotficationHub stockTicker)
        {
            _stockTicker = stockTicker;
        }

        public string GetMonitorState()
        {
            return _stockTicker.MonitorState.ToString();
        }

        public void StartMonitor()
        {
            _stockTicker.StartMonitor();

        }

        public void StopMonitor()
        {
            _stockTicker.StopMonitor();
        }

        public void ChangeBroadcastInterval(long milisecs)
        {
            if (milisecs < 500)
                return;

            _stockTicker.BroadcastInterval = TimeSpan.FromMilliseconds(milisecs);
        }
    }
}
