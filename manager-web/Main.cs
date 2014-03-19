using System;
using System.Net;
using System.Threading;
using Nancy.Hosting.Self;
using System.Diagnostics;
using System.Net.Sockets;

namespace MonoCounters.Web
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            args = new string[] { "/Users/ludovic/Xamarin/counters-sampler/manager-web" };

            if (args.Length < 1)
            {
                Console.WriteLine("Require at least 1 argument : rootpath");
                Environment.Exit(1);
            }

            var inspector = new Inspector(new TcpListener(IPAddress.Any, 8888));
            var inspectorThread = new Thread(inspector.Run);
            var history = new History(inspector);

            inspectorThread.Start();

            using (var nancyHost = new NancyHost(new Uri("http://localhost:8080/"), new Uri("http://127.0.0.1:8080/")))
            {
                nancyHost.Start();

                Console.WriteLine("Nancy now listening - navigating to http://localhost:8888/. Press enter to stop");
                Process.Start("http://localhost:8888/");
                Console.ReadKey();

                inspectorThread.Abort();
            }
        }
    }
}
