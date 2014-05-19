using System;
using System.Collections.Generic;
using System.IO;

namespace MonoCounters.Common.Agent
{
	public class Arguments
	{
		public string Assembly { get; private set; }

		public string Class { get; private set; }

		public List<string> BenchmarkArguments { get; private set; }

		public static Arguments Parse (Stream stream)
		{
			var arguments = new Arguments () { BenchmarkArguments = new List<string> () };

			using (var reader = new StreamReader (stream)) {
				while (!reader.EndOfStream) {
					var split = reader.ReadLine ().Split ('=');
					var value = split [1];

					switch (split [0]) {
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
					}
				}
			}

			return arguments;
		}
	}
}

