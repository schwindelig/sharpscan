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

            using (var scanner = new SharpScanner
            {
                PhantomJsDirectory = "C:\\tools\\phantomjs-2.1.1-windows\\bin"
            })
            {
                // Do a scan for some common ports
                System.Console.WriteLine("Run scan ...");

                var result = scanner.RunTcpScan(
                    IPAddress.Parse("82.220.1.1"),
                    IPAddress.Parse("82.220.255.255"),
                    new[] { 21, 80, 3389 });

                var onlineWithOpen = result.ScanEntries.Where(e => e.IsOnline && e.OpenPorts.Any()).ToList();
                foreach (var entry in onlineWithOpen)
                {
                    System.Console.WriteLine($"{entry.IpAddress}: {{{string.Join(";", entry.OpenPorts)}}}");
                }

                // Visit pages with port 80 open and take a screenshot
                System.Console.WriteLine("Start taking screenshots ...");

                var entriesWith80 = onlineWithOpen.Where(e => e.OpenPorts.Contains(80));
                foreach (var entry in entriesWith80)
                {
                    scanner.TakeScreenshot($"http://{entry.IpAddress}", screenshotDirectory, $"{entry.IpAddress}-80.png");
                }
            }
        }
    }
}
