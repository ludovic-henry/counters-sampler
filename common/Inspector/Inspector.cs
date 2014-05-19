using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using XamarinProfiler.Core.Reader;

namespace MonoCounters.Common.Inspector
{
	public class Inspector
	{
		BaseLogReader Reader;
		InspectorEventListener Listener;

		public delegate void SampleEventHandler (object sender, SampleEventArgs e);
		public event SampleEventHandler Sample;
		public event SampleEventHandler UpdatedSample;

		public delegate void InitEventHandler (object sender, InitEventArgs e);
		public event InitEventHandler Init;

		public Inspector (string filename)
		{
			Reader = new BaseLogReader (filename);
			Listener = new InspectorEventListener (this);
		}

		public void Run ()
		{
 			Reader.OpenReader ();

			while (true) {
				var buffer = Reader.TryReadBuffer (null, Listener);
				if (Reader.IsEof)
					break;
				if (buffer == null)
					throw new Exception ();
			}
		}

		public class SampleEventArgs : EventArgs
		{
			public ulong Timestamp { get; internal set; }
			public List<Counter> Counters  { get; internal set; }
		}

		public class InitEventArgs : EventArgs
		{
			public List<Counter> Counters  { get; internal set; }
		}

		class InspectorEventListener : EventListener
		{
			Inspector Inspector;

			Dictionary<ulong, Counter> Counters = new Dictionary<ulong, Counter> ();

			public InspectorEventListener (Inspector inspector)
			{
				Inspector = inspector;
			}

			public override void HandleSampleCountersDesc (List<Tuple<ulong, string, ulong, ulong, ulong, ulong>> counters)
			{
				foreach (var t in counters) {
					Counters.Add (t.Item6, new Counter () {
						Category = (Category)t.Item1,
						Name = t.Item2,
						Type = (Type)t.Item3,
						Unit = (Unit)t.Item4,
						Variance = (Variance)t.Item5,
						Index = t.Item6
					});
				}
			}

			public override void HandleSampleCounters (ulong timestamp, List<Tuple<ulong, ulong, object>> values)
			{
				var counters = values.ConvertAll<Counter> (t => {
					var counter = new Counter (Counters [t.Item1]);

					if (counter.Value == null) {
						counter.Value = t.Item3;
					} else {
						switch ((Common.Type)t.Item2) {
						case Common.Type.Int:
						case Common.Type.Long:
						case Common.Type.Word:
						case Common.Type.TimeInterval:
							counter.Value = (long)(counter.Value) + (long)(t.Item3);
							break;
						case Common.Type.UInt:
						case Common.Type.ULong:
							counter.Value = (ulong)(counter.Value) + (ulong)(t.Item3);
							break;
						case Common.Type.Double:
							counter.Value = (double)(t.Item3);
							break;
						case Common.Type.String:
							counter.Value = (string)(t.Item3);
							break;
						}
					}

					return Counters [t.Item1] = counter;
				});

				if (Inspector.UpdatedSample != null)
					Inspector.UpdatedSample (this, new SampleEventArgs () { Timestamp = timestamp, Counters = counters });

				if (Inspector.Sample != null)
					Inspector.Sample (this, new SampleEventArgs { Timestamp = timestamp, Counters = new List<Counter> (Counters.Values) });
			}
		}
	}
}

