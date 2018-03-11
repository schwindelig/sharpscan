using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetTools;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium;
using System.Text;

namespace SharpScan.Core
{
    public class SharpScanner : IDisposable
    {
        public int ConnectionTimeout { get; set; } = 100;

        public string PhantomJsDirectory { get; set; }

        protected PhantomJSDriver PhantomJsDriver { get; set; }


        public ScanResult RunTcpScan(IPAddress startAddress, IPAddress endAddress, IEnumerable<int> ports)
        {
            var startDate = DateTime.Now;

            this.DumpICanHazIpDotCom();

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
                    IpAddress = ScanIpAddress.FromIpAddress(ip),
                    OpenPorts = openPorts
                });
            });
            var endDate = DateTime.Now;

            return new ScanResult
            {
                StartAddress = ScanIpAddress.FromIpAddress(startAddress),
                EndAddress = ScanIpAddress.FromIpAddress(endAddress),
                ScanEntries = scanEntries,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public void TakeScreenshot(string url, string outputDirectory, string fileName)
        {
            if (this.PhantomJsDriver == null)
            {
                this.PhantomJsDriver = new PhantomJSDriver(this.PhantomJsDirectory, new PhantomJSOptions
                {
                    AcceptInsecureCertificates = true,
                    PageLoadStrategy = PageLoadStrategy.Eager,
                    UnhandledPromptBehavior = UnhandledPromptBehavior.Dismiss
                }, TimeSpan.FromSeconds(10));

                this.PhantomJsDriver.Manage().Window.Size = new Size(1024, 768);
            }
            try
            {
                this.PhantomJsDriver.Navigate().GoToUrl(url);
                this.PhantomJsDriver.GetScreenshot()
                    .SaveAsFile($"{outputDirectory}\\{fileName}", ScreenshotImageFormat.Png);
            }
            catch (Exception e)
            {
            }
        }

        public void DumpICanHazIpDotCom()
        {
            var response = null as string;
            using (var client = new TcpClient())
            {
                client.Connect("icanhazip.com", 80);
                using (var stream = client.GetStream())
                {
                    var headers = $@"GET http://icanhazip.com/ HTTP/1.1
Host: icanhazip.com
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:60.0) Gecko/20100101 Firefox/60.0
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
Accept-Language: en-US,de;q=0.7,en;q=0.3
Accept-Encoding: gzip, deflate
Connection: keep-alive
Upgrade-Insecure-Requests: 1
Pragma: no-cache
Cache-Control: no-cache

";
                    stream.Write(Encoding.UTF8.GetBytes(headers), 0, headers.Length);
                    stream.Flush();

                    var responseBytes = new byte[client.ReceiveBufferSize];
                    stream.Read(responseBytes, 0, client.ReceiveBufferSize);
                    response = Encoding.UTF8.GetString(responseBytes);

                    stream.Close();
                }
                client.Close();
            }

            if (string.IsNullOrWhiteSpace(response)) return;
            response = response.Replace("\0", string.Empty);
            foreach (var line in response.Split("\r\n"))
            {
                var cleanline = line.Replace("\r", string.Empty).Replace("\n", string.Empty);
                if(IPAddress.TryParse(cleanline, out var ip))
                {
                    Console.WriteLine($"Public IP: {ip}");
                    break;
                }
            }
        }

        public void Dispose()
        {
            this.PhantomJsDriver?.Quit();
        }
    }
}
