using System;
using System.Linq;
using System.Threading;
using HidSharp;
using HidSharp.Reports.Input;
using TabletDriverLib.Tablet;

namespace TabletDriverLib
{
    public class TabletReader : DeviceReader<IDeviceReport>
    {
        public TabletReader(HidDevice tablet)
        {
            Tablet = tablet;
            WorkerThread = new Thread(Main)
            {
                Name = "OpenTabletDriver Tablet Reader",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
            };
        }

        public HidDevice Tablet { private set; get; }
        public override IReportParser<IDeviceReport> Parser { set; get; }

        private Thread WorkerThread;

        public override void Start()
        {
            Setup();
            WorkerThread.Start();
        }

        private void Setup()
        {
            var config = new OpenConfiguration();
            config.SetOption(OpenOption.Priority, OpenPriority.Low);
            for (int retries = 3; retries > 0; retries--)
            {
                if (Tablet.TryOpen(config, out var stream, out var exception))
                {
                    ReportStream = (HidStream)stream;
                    break;
                }
                else
                {                    
                    Log.Write("Detect", $"{exception}; Retrying {retries} more times", true);
                    Thread.Sleep(1000);
                }
            }
            
            if (ReportStream == null)
            {
                Log.Write("Detect", "Failed to open tablet. Make sure you have required permissions to open device streams.", true);
                return;
            }

            if (Driver.Debugging)
            {
                Log.Debug("InputReportLength: " + Tablet.GetMaxInputReportLength());
            }
        }

        private static string GetFormattedTime()
        {
            var hr = string.Format(GetNumberFormat(2), DateTime.Now.Hour);
            var min = string.Format(GetNumberFormat(2), DateTime.Now.Minute);
            var sec = string.Format(GetNumberFormat(2), DateTime.Now.Second);
            var ms = string.Format(GetNumberFormat(3), DateTime.Now.Millisecond);
            return $"{hr}:{min}:{sec}.{ms}";
        }

        private static string GetNumberFormat(int digits)
        {
            var zeros = Enumerable.Repeat('0', digits).ToArray();
            return "{0:" + new string(zeros) + "}";
        }
    }
}