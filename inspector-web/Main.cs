using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Nancy;
using Nancy.Hosting.Self;
using MonoCounters.Web.Models;

namespace MonoCounters.Web
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var thread = new Thread (() => {
				var listener = new TcpListener (IPAddress.Any, 8888);
				listener.Start ();

				while (true) {
					var socket = listener.AcceptSocket ();
					var stream = new NetworkStream (socket);
					var inspector = new Inspector (stream);
					var history = new History (inspector);

					InspectorModel.Inspector = inspector;
					HistoryModel.History = history;

					inspector.Run ();
				}
			});

			thread.Start ();

			using (var nancyHost = new NancyHost (new Uri ("http://127.0.0.1:8080/"), new Uri ("http://localhost:8080/"))) {
				StaticConfiguration.DisableErrorTraces = false;
				StaticConfiguration.Caching.EnableRuntimeViewUpdates = true;

				nancyHost.Start ();

				Console.WriteLine ("Nancy now listening - navigating to http://127.0.0.1:8080/. Press key to stop");

				while (true) {
					if (Console.ReadKey ().Key == ConsoleKey.Enter) {
						break;
					}

					Thread.Sleep (100);
				}
			}
		}
	}
}
