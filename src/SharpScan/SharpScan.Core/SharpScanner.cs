using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetTools;

namespace SharpScan.Core
{
    public class SharpScanner
    {
        public int ConnectionTimeout { get; set; } = 100;

        public ScanResult RunTcpScan(IPAddress startAddress, IPAddress endAddress, IEnumerable<int> ports)
        {
            var startDate = DateTime.Now;
            var range = new IPAddressRange(startAddress, endAddress) as IEnumerable<IPAddress>;
            var scanEntries = new ConcurrentStack<ScanEntry>();
            Parallel.ForEach(range, ip =>
            {
                var isOnline = false;
                var openPorts = new List<int>();

                var ping = new Ping();
                var pingResult = ping.Send(ip);
                if (pingResult != null && pingResult.Status == IPStatus.Success)
                {
                    isOnline = true;

                    foreach (var port in ports)
                    {
                        var client = new TcpClient();
                        if (client.ConnectAsync(ip, port).Wait(this.ConnectionTimeout))
                        {
                            openPorts.Add(port);
                        }
                        client.Close();
                    }
                }

                scanEntries.Push(new ScanEntry
                {
                    IsOnline = isOnline,
                    IpAddress = ip,
                    OpenPorts = openPorts
                });
            });
            var endDate = DateTime.Now;

            return new ScanResult
            {
                StartAddress = startAddress,
                EndAddress = endAddress,
                ScanEntries = scanEntries,
                StartDate = startDate,
                EndDate = endDate
            };
        }
    }
}
