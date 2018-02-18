using System.Linq;
using System.Net;
using SharpScan.Core;

namespace SharpScan.Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            var scanner = new SharpScanner();
            var result = scanner.RunTcpScan(
                IPAddress.Parse("82.220.1.1"),
                IPAddress.Parse("82.220.30.255"),
                new[] { 21, 80, 27017, 3389 });

            var onlineWithOpen = result.ScanEntries.Where(e => e.IsOnline && e.OpenPorts.Any());
            foreach (var entry in onlineWithOpen)
            {
                System.Console.WriteLine($"{entry.IpAddress}: {{{string.Join(";", entry.OpenPorts)}}}");
            }
        }
    }
}
