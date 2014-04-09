using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MonoCounters
{
	public class Inspector : IDisposable
	{
		public Boolean Closed { get; private set; }

		public delegate void TickEventHandler (object sender,TickEventArgs e);
		public event TickEventHandler SampleTick;

		delegate void StreamCallback ();

		Dictionary<string, Queue<StreamCallback>> callbacks;
		byte[] buffer;
		Dictionary<short, Counter> counters;
		Stream stream;

		public Inspector (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			this.stream = stream;

			counters = new Dictionary<short, Counter> ();

			buffer = new byte[12];

			callbacks = new Dictionary<string, Queue<StreamCallback>> ();
			callbacks.Add ("list", new Queue<StreamCallback> ());
			callbacks.Add ("add", new Queue<StreamCallback> ());
			callbacks.Add ("remove", new Queue<StreamCallback> ());

			Closed = false;
		}

		public Task<List<Tuple<string, string>>> ListCounters ()
		{
			var promise = new TaskCompletionSource<List<Tuple<string,string>>> ();

			lock (stream) {
				WriteBufferToStream (BitConverter.GetBytes ((byte)0), 1);

				callbacks ["list"].Enqueue (() => {
					var counters = new List<Tuple<string,string>> ();

					while (true) {
						var cat = ReadStreamToString ();
						if (cat == null)
							break;
						var name = ReadStreamToString ();
						counters.Add (Tuple.Create (cat, name));
					}

					promise.SetResult (counters);
				});
			}

			return promise.Task;
		}

		public Task<Tuple<Counter, ResponseStatus>> AddCounter (string category, string name)
		{
			var promise = new TaskCompletionSource<Tuple<Counter, ResponseStatus>> ();

			lock (stream) {
				WriteBufferToStream (BitConverter.GetBytes ((byte)1), 1);
				WriteStringToStream (category);
				WriteStringToStream (name);

				callbacks ["add"].Enqueue (() => {
					Counter counter = null;
					ResponseStatus status = (ResponseStatus)ReadStreamToBuffer (buffer, 1) [0];

					if (status == ResponseStatus.OK) {
						counter = ReadStreamToCounter ();

						if (counters.ContainsKey (counter.Index))
							throw new Exception ();

						counters.Add (counter.Index, counter);

					}

					promise.SetResult (Tuple.Create<Counter, ResponseStatus> (counter, status));
				});
			}

			return promise.Task;
		}

		public Task<ResponseStatus> RemoveCounter (short index)
		{
			var promise = new TaskCompletionSource<ResponseStatus> ();

			lock (stream) {
				WriteBufferToStream (BitConverter.GetBytes ((byte)2), 1);
				WriteBufferToStream (BitConverter.GetBytes (index), 2);

				callbacks ["remove"].Enqueue (() => {
					promise.SetResult ((ResponseStatus)ReadStreamToBuffer (buffer, 1) [0]);
				});
			}

			return promise.Task;
		}

		public void Run ()
		{
			if (Closed)
				return;

			try {
				while (true) {
					byte cmd = ReadStreamToBuffer (buffer, 1) [0];

					switch (cmd) {
					case 0: // Hello
						short version = BitConverter.ToInt16 (ReadStreamToBuffer (buffer, 2), 0);
						short count = BitConverter.ToInt16 (ReadStreamToBuffer (buffer, 2), 0);

						for (short i = 0; i < count; i++) {
							var counter = ReadStreamToCounter ();

							counters.Add (counter.Index, counter);
						}
						break;
					case 1: // List
						lock (stream)
							callbacks ["list"].Dequeue () ();
						break;
					case 2: // Add
						lock (stream)
							callbacks ["add"].Dequeue () ();
						break;
					case 3: // Remove
						lock (stream)
							callbacks ["remove"].Dequeue () ();
						break;
					case 4: // Sampling
						long timestamp = BitConverter.ToInt64 (ReadStreamToBuffer (buffer, 8), 0);

						while (true) {
							short index = BitConverter.ToInt16 (ReadStreamToBuffer (buffer, 2), 0);

							if (index < 0)
								break;

							Counter counter = new Counter (counters [index]);

							short size = BitConverter.ToInt16 (ReadStreamToBuffer (buffer, 2), 0);

							switch (counter.Type) {
							case Type.Int:
								counter.Value = BitConverter.ToInt32 (ReadStreamToBuffer (buffer, 4), 0);
								break;
							case Type.Word:
								counter.Value = (size == 4) ?
									BitConverter.ToInt32 (ReadStreamToBuffer (buffer, 4), 0) :
									BitConverter.ToInt64 (ReadStreamToBuffer (buffer, 8), 0);
								break;
							case Type.Long:
								counter.Value = BitConverter.ToInt64 (ReadStreamToBuffer (buffer, 8), 0);
								break;
							case Type.Double:
								counter.Value = BitConverter.ToDouble (ReadStreamToBuffer (buffer, 8), 0);
								break;
							}

							counters [index] = counter;
						}

						if (SampleTick != null)
							SampleTick (this, new TickEventArgs (timestamp, new List<Counter> (counters.Values)));

						break;
					}
				}
			} catch (IOException e) {
				Debug.WriteLine ("End of the stream. Exception : " + e.ToString (), "MonoCounters.Inspector.Run");
			} catch (ObjectDisposedException e) {
				Debug.WriteLine ("End of the stream. Exception : " + e.ToString (), "MonoCounters.Inspector.Run");
			} finally {
				Closed = true;
				Close ();

				counters.Clear ();
			}

		}

		public void Close ()
		{
			try {
				Closed = true;

				if (stream.CanWrite) {
					lock (stream)
						WriteBufferToStream (new byte[] { 127 }, 1);
				}
			} finally {
				stream.Close ();
			}
		}

		public void Dispose ()
		{
			this.Close ();
		}

		byte[] ReadStreamToBuffer (byte[] buffer, int size)
		{
			int total = 0, received;

			while (total < size) {
				received = stream.Read (buffer, total, size - total);

				if (received <= 0)
					throw new IOException ();

				total += received;
			}

			return buffer;
		}

		void WriteBufferToStream (byte[] buffer, int size)
		{
			stream.Write (buffer, 0, size);
		}

		Counter ReadStreamToCounter ()
		{
			var counter = new Counter ();

			counter.Category = (Category)BitConverter.ToInt32 (ReadStreamToBuffer (buffer, 4), 0);
			counter.Name = ReadStreamToString ();
			counter.Type = (Type)BitConverter.ToInt32 (ReadStreamToBuffer (buffer, 4), 0);
			counter.Unit = (Unit)BitConverter.ToInt32 (ReadStreamToBuffer (buffer, 4), 0);
			counter.Variance = (Variance)BitConverter.ToInt32 (ReadStreamToBuffer (buffer, 4), 0);
			counter.Index = BitConverter.ToInt16 (ReadStreamToBuffer (buffer, 2), 0);

			return counter;
		}

		void WriteStringToStream (string value)
		{
			if (value == null) {
				WriteBufferToStream (BitConverter.GetBytes (-1), 4);
			} else {
				WriteBufferToStream (BitConverter.GetBytes (value.Length), 4);
				WriteBufferToStream (Encoding.Default.GetBytes (value), value.Length);
			}
		}

		string ReadStreamToString ()
		{
			int length = BitConverter.ToInt32 (
				                      ReadStreamToBuffer (buffer, 4), 0);

			if (length < 0)
				return null;

			if (length > buffer.Length)
				buffer = new byte[length];

			return Encoding.Default.GetString (
				ReadStreamToBuffer (buffer, length), 0, length);
		}

		public enum ResponseStatus
		{
			OK,
			NOK,
			NOTFOUND,
			EXISTING,
		};

		public class TickEventArgs : EventArgs
		{
			public List<Counter> Counters  { get; private set; }

			public long          Timestamp { get; private set; }

			public TickEventArgs (long timestamp, List<Counter> counters)
			{
				Counters = counters;
				Timestamp = timestamp;
			}
		}
	}
}

