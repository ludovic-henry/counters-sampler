using System;
using System.Threading;
using Nancy;
using Nancy.Hosting.Self;

namespace MonoCounters.Web
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Models.Benchmark.Initialize ();
			Models.Configuration.Initialize ();
			Models.Counter.Initialize ();
			Models.Device.Initialize ();
			Models.Project.Initialize ();
			Models.Recipe.Initialize ();
			Models.Revision.Initialize ();
			Models.Run.Initialize ();
			Models.Sample.Initialize ();

			var nancyHost = new NancyHost (new Uri ("http://127.0.0.1:8080/"));

			nancyHost.Start ();

				StaticConfiguration.DisableErrorTraces = false;
				StaticConfiguration.Caching.EnableRuntimeViewUpdates = true;

				Console.WriteLine ("Nancy now listening on http://127.0.0.1:8080/. Press key to stop");

				while (true) {
					if (Console.ReadKey ().Key == ConsoleKey.Enter) {
						break;
					}

					Thread.Sleep (100);
				}

			nancyHost.Stop ();
			nancyHost.Dispose ();

			Console.WriteLine ("The End;");
		}
	}
}
