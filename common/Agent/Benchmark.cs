using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;

namespace MonoCounters.Common.Agent
{
	public class Benchmark
	{
		Assembly Assembly { get; set; }

		string Klass { get; set; }

		//PerformanceCounter Counter { get; set; }

		public Benchmark (Assembly assembly, string klass)
		{
			if (assembly == null)
				throw new ArgumentNullException ("assembly");
			if (klass == null)
				throw new ArgumentNullException ("klass");

			Assembly = assembly;
			Klass = klass;

			/*var category = PerformanceCounterCategory.Create ("Custom", "Custom Counters", 
				PerformanceCounterCategoryType.SingleInstance, "Benchmark Running", "Is Benchrmark Running ?");

			Counter = new PerformanceCounter ("Custom", "Benchmark Running") { RawValue = 0 };*/
		}

		public long Run (string[] arguments)
		{
			if (arguments == null)
				throw new ArgumentNullException ("arguments");

			var type = Assembly.GetType (Klass);
			var method = type.GetMethod ("Main", new System.Type[] { typeof (string[]) });

			var stopwatch = new Stopwatch ();

			//Counter.RawValue = 1;

			stopwatch.Start ();
			method.Invoke (null, new object[] { arguments });
			stopwatch.Stop ();

			//Counter.RawValue = 0;

			return stopwatch.ElapsedTicks;
		}
	}
}

