using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace MonoCounters
{
    public class Inspector
    {
        public delegate void TickEventHandler (object sender, TickEventArgs e);
        public event TickEventHandler Tick;

        private TcpListener listener;
        private Dictionary<short, Counter> counters;

        public Boolean Enable { get; set; }

        public Inspector(TcpListener listener)
        {
            this.listener = listener;
            this.counters = new Dictionary<short, Counter>();
       
            Enable = true;
        }

        public void Run()
        {
            Debug.WriteLine("Start", "MonoCounters.Inspector");
            listener.Start();

            Socket socket = null;
            var    buffer = new byte[12];

            try {
                socket = listener.AcceptSocket();

                Debug.WriteLine("Connection Open", "MonoCounters.Inspector");

                // parse headers

                if (!ReadStreamToBuffer(socket, buffer, 2))
                    throw new Exception();
                short count = BitConverter.ToInt16(buffer, 0);

                for (short i = 0; i < count; i++)
                {
                    var counter = new Counter();

                    if (!ReadStreamToBuffer(socket, buffer, 4))
                        throw new Exception();
                    counter.Category = (Category)BitConverter.ToInt32(buffer, 0);

                    if (!ReadStreamToBuffer(socket, buffer, 4))
                        throw new Exception();
                    int length = BitConverter.ToInt32(buffer, 0);

                    if (length > buffer.Length)
                        buffer = new byte[length];

                    if (!ReadStreamToBuffer(socket, buffer, length))
                        throw new Exception();
                    counter.Name = Encoding.Default.GetString(buffer, 0, length);

                    if (!ReadStreamToBuffer(socket, buffer, 4))
                        throw new Exception();
                    counter.Type = (Type)BitConverter.ToInt32(buffer, 0);

                    if (!ReadStreamToBuffer(socket, buffer, 4))
                        throw new Exception();
                    counter.Unit = (Unit)BitConverter.ToInt32(buffer, 0);

                    if (!ReadStreamToBuffer(socket, buffer, 4))
                        throw new Exception();
                    counter.Variance = (Variance)BitConverter.ToInt32(buffer, 0);

                    if (!ReadStreamToBuffer(socket, buffer, 2))
                        throw new Exception();
                    counter.Index = BitConverter.ToInt16(buffer, 0);

                    this.counters.Add(counter.Index, counter);
                }


                while (Enable)
                {
                    if (!ReadStreamToBuffer(socket, buffer, 8))
                        throw new Exception();
                    var timestamp = BitConverter.ToInt64(buffer, 0);

                    // parse values

                    while (true)
                    {
                        if (!ReadStreamToBuffer(socket, buffer, 2))
                            throw new Exception();
                        short index = BitConverter.ToInt16(buffer, 0);

                        if (index < 0)
                            break;

                        Counter counter = new Counter(this.counters[index]);

                        if (!ReadStreamToBuffer(socket, buffer, 2))
                            throw new Exception();
                        short size = BitConverter.ToInt16(buffer, 0);

                        if (!ReadStreamToBuffer(socket, buffer, size))
                            throw new Exception();

                        switch (counter.Type) {
                            case Type.MONO_COUNTER_TYPE_INT:
                                counter.Value = BitConverter.ToInt32(buffer, 0);
                                break;
                            case Type.MONO_COUNTER_TYPE_WORD:
                                counter.Value = (size == 4) ? BitConverter.ToInt32(buffer, 0) : BitConverter.ToInt64(buffer, 0);
                                break;
                            case Type.MONO_COUNTER_TYPE_LONG:
                                counter.Value = BitConverter.ToInt64(buffer, 0);
                                break;
                            case Type.MONO_COUNTER_TYPE_DOUBLE:
                                counter.Value = BitConverter.ToDouble(buffer, 0);
                                break;
                        }

                        this.counters[index] = counter;
                    }

                    if (Tick != null)
                    {
                        Tick(this, new TickEventArgs(timestamp, new List<Counter>(this.counters.Values)));
                    }
                }
            } finally {
                if (socket != null)
                    socket.Close();

                this.counters.Clear();
            }
            Debug.WriteLine("Connection Closed", "MonoCounters.Inspector");
        }

        private static Boolean ReadStreamToBuffer(Socket socket, byte[] buffer, int size)
        {
            int read = 0, received;

            while (read < size)
            {
                received = socket.Receive(buffer, read, size - read, SocketFlags.None);

                if (received < 0)
                    return false;

                read += received;
            }

            return true;
        }

        public class TickEventArgs : EventArgs
        {
            public List<Counter> Counters  { get; private set; }
            public long          Timestamp { get; private set; }

            public TickEventArgs(long timestamp, List<Counter> counters)
            {
                Counters = counters;
                Timestamp = timestamp;
            }
        }
    }
}

