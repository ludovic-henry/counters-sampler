using System;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using Nancy;
using Nancy.Hosting.Self;
//using MonoCounters;
using MonoCounters.Web.Models;

namespace MonoCounters.Web
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var inspector = new Inspector(new TcpListener(IPAddress.Any, 8888));
            var history = new History(inspector);
            var thread = new Thread(inspector.Run);

            thread.Start();

            InspectorModel.Initialize(inspector);
            HistoryModel.Initialize(history);

            using (var nancyHost = new NancyHost(new Uri("http://127.0.0.1:8080/")))
            {
                StaticConfiguration.DisableErrorTraces = false;
                StaticConfiguration.Caching.EnableRuntimeViewUpdates = true;

                nancyHost.Start();

                Console.WriteLine("Nancy now listening - navigating to http://127.0.0.1:8080/. Press ctrl+c to stop");

                while (true)
                {
                    if (Console.ReadKey().Key == ConsoleKey.Enter)
                        break;
                }
            }
        }
    }
}
