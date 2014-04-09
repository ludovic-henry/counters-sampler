using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;

namespace MonoCounters.Agent
{
	public class Benchmark
	{
		Assembly Assembly { get; set; }

		string Klass { get; set; }

		public Benchmark (Assembly assembly, string klass)
		{
			if (assembly == null)
				throw new ArgumentNullException ("assembly");
			if (klass == null)
				throw new ArgumentNullException ("klass");

			Assembly = assembly;
			Klass = klass;
		}

		public long Run (string[] arguments)
		{
			if (arguments == null)
				throw new ArgumentNullException ("arguments");

			var type = Assembly.GetType (Klass);
			var method = type.GetMethod ("Main", new System.Type[] { typeof (string[]) });

			var stopwatch = new Stopwatch ();

			stopwatch.Start ();
			method.Invoke (null, new object[] { arguments });
			stopwatch.Stop ();

			return stopwatch.ElapsedTicks;
		}
	}
}

