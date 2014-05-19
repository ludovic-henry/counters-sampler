using System;
using System.IO;
using System.Collections.Generic;

namespace MonoCounters.Inspector
{
	public class MainClass
	{
		public static void Main (string[] args)
		{
			if (args.Length < 1)
				throw new ArgumentException ("usage : samples");

			var format = "| {0,-18} | {1,-13} | {2,-41} | {3,-18} | {4,-18} | {5,-13} | {6,-10} |";

//			var stream = new FileStream (args [0], FileMode.Open);
			var inspector = new Common.Inspector.Inspector (args [0]);


			Console.WriteLine (format, "Timestamp", "Category", "Name", "Value", "Value HEX", "Type", "Unit");
			Console.WriteLine ("--------------------------------------------------------------------" +
				"-------------------------------------------------------------------------------------");

			inspector.UpdatedSample += (sender, e) => {
				var timestamp = new TimeSpan (0, 0, 0, 0, Convert.ToInt32 (e.Timestamp)).ToString ("G");
				foreach (var c in e.Counters) {
					object value;
					string hexvalue = "";

					if (c.Type == Common.Type.Long && c.Unit == Common.Unit.Time)
						value = new TimeSpan (Convert.ToInt64 (c.Value)).ToString ("G");
					else if (c.Type == Common.Type.TimeInterval)
						value = new TimeSpan (Convert.ToInt64 (c.Value) * 10).ToString ("G");
					else
						value = c.Value;

					switch (c.Type) {
					case Common.Type.String:
						hexvalue = "";
						break;
					case Common.Type.Double:
						hexvalue = "0x" + String.Join ("", new List<byte> (BitConverter.GetBytes ((double) c.Value)).ConvertAll<string>(b => b.ToString("x2")));
						break;
					case Common.Type.Int:
					case Common.Type.UInt:
						hexvalue =  String.Format ("0x{0:x8}", c.Value);
						break;
					default:
						hexvalue =  String.Format ("0x{0:x16}", c.Value);
						break;
					}

					Console.WriteLine (format, timestamp, c.CategoryName, c.Name, value, hexvalue, c.TypeName, c.UnitName);
				}
			};

			inspector.Run ();
		}
	}
}

