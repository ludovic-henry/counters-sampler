using System;
using System.Collections.Generic;
using System.IO;

namespace MonoCounters.Agent
{
	public class Arguments
	{
		public int Interval { get; private set; }

		public string Address { get; private set; }

		public string Counters { get; private set; }

		public string Assembly { get; private set; }

		public string Class { get; private set; }

		public List<string> BenchmarkArguments { get; private set; }

		public static Arguments Parse (Stream stream)
		{
			var arguments = new Arguments ();

			arguments.BenchmarkArguments = new List<string> ();

			using (var reader = new StreamReader (stream)) {
				while (!reader.EndOfStream) {
					var split = reader.ReadLine ().Split ('=');
					var value = split [1];

					switch (split [0]) {
					case "interval":
						arguments.Interval = Int32.Parse (value);
						break;
					case "address":
						arguments.Address = value;
						break;
					case "counters":
						arguments.Counters = value;
						break;
					case "assembly":
						arguments.Assembly = value;
						break;
					case "class":
						arguments.Class = value;
						break;
					case "arguments":
						arguments.BenchmarkArguments.Add (value);
						break;
					default:
						throw new ArgumentException (String.Format ("Unknown argument '{0}'", split [0]));
						break;
					}
				}
			}

			return arguments;
		}
	}
}

