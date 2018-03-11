using System.Collections.Generic;
using System.Net;

namespace SharpScan.Core
{
    public class ScanEntry
    {
        public bool IsOnline { get; set; }

        public ScanIpAddress IpAddress { get; set; }

        public IEnumerable<int> OpenPorts { get; set; }
    }
}