using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ROOT.CIMV2.Win32;

namespace NetworkCardRestart
{
    class Program
    {
        private const int MillisecondsTimeout = 3000;

        static void Main(string[] args)
        {
            while (!PingTest())
            {
                RestartAdapter();
            }
            Console.ReadLine();
        }

        private static void RestartAdapter()
        {
            Console.WriteLine("Restarting Adapters...");
            SelectQuery query = new SelectQuery("Win32_NetworkAdapter", "NetConnectionStatus=2");
            ManagementObjectSearcher search = new ManagementObjectSearcher(query);
            ArrayList threads = new ArrayList();
            foreach (ManagementObject result in search.Get())
            {
                Thread t = new Thread(() =>
                {
                    NetworkAdapter adapter = new NetworkAdapter(result);

                    // Identify the adapter you wish to disable here. 
                    // In particular, check the AdapterType and 
                    // Description properties.

                    // Here, we're selecting the LAN adapters.
                    if (adapter.AdapterType.Equals("Ethernet 802.3"))
                    {
                        adapter.Disable();
                        Console.WriteLine("Waiting " + MillisecondsTimeout / 1000 + " seconds to activate the card.");
                        Thread.Sleep(MillisecondsTimeout);
                        adapter.Enable();
                    }
                });
                t.Start();
                threads.Add(t);

            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }
        }

        // Ping www.google.com to check if the user has a internet connection.
        private static bool PingTest()
        {
            Console.WriteLine("Checking if Internet is avaliable...");

            Ping ping = new Ping();

            PingReply pingStatus = ping.Send(IPAddress.Parse("google.com"));

            if (pingStatus != null && pingStatus.Status == IPStatus.Success)
            {
                Console.WriteLine("We have Internet access.");
                return true;
            }
            else
            {
                Console.WriteLine("We don't have Internet access.");
                return false;
            }
        }
    }
}
