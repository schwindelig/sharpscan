using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace SharpScan.Core
{
    public class ScanResult
    {
        public ScanIpAddress StartAddress { get; set; }

        public ScanIpAddress EndAddress { get; set; }

        public IEnumerable<ScanEntry> ScanEntries { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public void Save(string path)
        {
            var serialized = JsonConvert.SerializeObject(this);
            File.WriteAllText(path, serialized);
        }

        public static ScanResult Load(string path)
        {
            var serialized = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ScanResult>(serialized);
        }
    }
}