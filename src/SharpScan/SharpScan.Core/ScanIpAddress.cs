using System.Net;

namespace SharpScan.Core
{
    public class ScanIpAddress
    {
        public string Ip { get; set; }

        public static ScanIpAddress FromIpAddress(IPAddress ipAddress)
        {
            return new ScanIpAddress
            {
                Ip = ipAddress.ToString()
            };
        }

        public IPAddress ToIpAddress()
        {
            return IPAddress.Parse(this.Ip);
        }
    }
}
