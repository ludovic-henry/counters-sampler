using System;
using System.Threading;
using System.IO;

using Nancy;
using Nancy.Hosting.Self;

using MonoCounters.Models;

namespace MonoCounters.Coordinator
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Database.OpenConnection (Path.Combine (Environment.CurrentDirectory, "..", "..", "database.sqlite"));

			using (var nancy = new NancyHost (new Uri ("http://127.0.0.1:8080/"))) {
				nancy.Start ();

				StaticConfiguration.DisableErrorTraces = false;
				StaticConfiguration.Caching.EnableRuntimeViewUpdates = true;

				Console.WriteLine ("Nancy now listening on http://127.0.0.1:8080/. Press key to stop");

				while (true) {
					if (Console.ReadKey ().Key == ConsoleKey.Enter)
						break;

					Thread.Sleep (100);
				}

				nancy.Stop ();
			}

			Console.WriteLine ("The End;");
		}
	}
}
