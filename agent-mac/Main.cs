using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Net;
using System.Collections.Generic;
using MonoCounters.Common.Agent;

namespace MonoCounters.Agent.Mac
{
	public class MainClass
	{


		public static void Main (string[] args)
		{
			if (args.Length < 3)
				throw new ArgumentException ("usage : root samples_file inspector_address");

			var root = args [0];
			var samplesFile = Path.IsPathRooted (args [1]) ? args [1] : Path.Combine (root, args [1]);
			var inspectorAddress = args [2];

			var arguments = Arguments.Parse (File.Open (Path.Combine (
				root, "arguments.ini"), FileMode.Open));

			var assembly = Path.IsPathRooted (arguments.Assembly) ?
				arguments.Assembly : Path.Combine (root, arguments.Assembly);

			var time = new Benchmark (Assembly.LoadFile (assembly), arguments.Class)
				.Run (arguments.BenchmarkArguments.ToArray ());

			using (var samples = File.Open (samplesFile, FileMode.Open)) {
				new ResultUploader (inspectorAddress).Upload (1, new FileStream (samplesFile, FileMode.Open));
			}

			Console.WriteLine ("{0:N3} ms", time / 10000d);
		}
	}
}
