using System;
using System.Linq;
using System.Net;
using SharpScan.Core;

namespace SharpScan.Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            const string screenshotDirectory = "c:\\temp\\screenshots";
            const string phantomJsDirectory = "c:\\tools\\phantomjs-2.1.1-windows\\bin";

            using (var scanner = new SharpScanner
            {
                PhantomJsDirectory = phantomJsDirectory
            })
            {
                // Do a scan for some common ports
                System.Console.WriteLine("Run scan ...");
                var scanResult = scanner.RunTcpScan(
                    IPAddress.Parse("82.220.83.135"),
                    IPAddress.Parse("82.220.83.137"),
                    new[] { 3389 });

                var onlineWithOpen = scanResult.ScanEntries.Where(e => e.IsOnline && e.OpenPorts.Any()).ToList();
                foreach (var entry in onlineWithOpen)
                {
                    System.Console.WriteLine($"{entry.IpAddress} {{{string.Join(";", entry.OpenPorts)}}}");
                }

                // Visit pages with port 80 open and take a screenshot
                System.Console.WriteLine("Start taking screenshots ...");
                var entriesWith80 = onlineWithOpen.Where(e => e.OpenPorts.Contains(80));
                foreach (var entry in entriesWith80)
                {
                    scanner.TakeScreenshot($"http://{entry.IpAddress}", screenshotDirectory, $"{entry.IpAddress}-80.png");
                }

                var path = $"c:\\temp\\scan-results\\{ Guid.NewGuid().ToString()}.json";
                scanResult.Save(path);

                var secondResult = ScanResult.Load(path);
            }
        }
    }
}
