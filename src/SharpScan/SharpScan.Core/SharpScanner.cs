using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetTools;
using Newtonsoft.Json;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium;

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
            catch(Exception e)
            {
            }
        }

        public void Dispose()
        {
            this.PhantomJsDriver?.Quit();
        }
    }
}
