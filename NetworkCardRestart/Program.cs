using System;
using System.Collections;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using ROOT.CIMV2.Win32;

namespace NetworkCardRestart
{
    class Program
    {
        private const int MillisecondsTimeoutDC = 1000 * 5;
        private const int MillisecondsTimeoutC = 1000 * 12;

        private const int TimesPing = 4;
        private const int AttemptsToRestart = 20;

        static void Main(string[] args)
        {
            uint counter = 0;
            while (!PingTest() && counter++ < AttemptsToRestart)
            {
                RestartAdapter();
            }
            if (counter == AttemptsToRestart)
            {
                Console.WriteLine("We have reached the maximum amount of tries.");
            }
            Console.WriteLine("Finishing.");
            //Console.ReadLine();
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
                        Console.WriteLine("Waiting " + MillisecondsTimeoutDC / 1000 + " seconds to activate the card.");
                        Thread.Sleep(MillisecondsTimeoutDC);
                        adapter.Enable();
                        Console.WriteLine("Waiting " + MillisecondsTimeoutC / 1000 +
                                          " seconds in order to get the card ready.");
                        Thread.Sleep(MillisecondsTimeoutC);
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

            try
            {
                IPAddress ip = IPAddress.Parse("8.8.8.8");

                for (var i = 0; i < TimesPing; ++i)
                {
                    var reply = ping.Send(ip);
                    Console.WriteLine("Reply from {0} Status: {1} time:{2}ms",
                                      reply.Address,
                                      reply.Status,
                                      reply.RoundtripTime);
                    if (reply.Status.ToString().Equals("Success"))
                    {
                        Console.WriteLine("We have Internet access.");
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Source);
                Console.WriteLine(e.Message);
                // Wait a little more
                Console.WriteLine("Waiting a little longer.");
                Thread.Sleep(MillisecondsTimeoutC);
                return false;
            }

            Console.WriteLine("We don't have Internet access.");
            return false;
        }
    }
}
