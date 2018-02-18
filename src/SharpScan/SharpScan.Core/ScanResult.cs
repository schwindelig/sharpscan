using System;
using System.Collections.Generic;
using System.Net;

namespace SharpScan.Core
{
    public class ScanResult
    {
        public IPAddress StartAddress { get; set; }

        public IPAddress EndAddress { get; set; }

        public IEnumerable<ScanEntry> ScanEntries { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}