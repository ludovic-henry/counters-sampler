using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Threading.Tasks;

namespace MonoCounters
{
    public class Inspector
    {
        public delegate void TickEventHandler (object sender, TickEventArgs e);
        public event TickEventHandler SampleTick;

        private delegate void SocketCallback (Socket socket);
        private Dictionary<string, Queue<SocketCallback>> callbacks;

        private TcpListener listener;
        private Socket socket;
        private byte[] buffer;

        private Dictionary<short, Counter> counters;

        public Boolean Enable { get; set; }

        public Inspector(TcpListener listener)
        {
            this.listener = listener;
            this.socket = null;
            this.buffer = new byte[12];

            this.counters = new Dictionary<short, Counter>();

            this.callbacks = new Dictionary<string, Queue<SocketCallback>>();
            this.callbacks.Add("list", new Queue<SocketCallback>());
            this.callbacks.Add("add", new Queue<SocketCallback>());
            this.callbacks.Add("remove", new Queue<SocketCallback>());
       
            Enable = true;
        }

        public Task<Tuple<List<Counter>, ResponseStatus>> ListCounters()
        {
            var promise = new TaskCompletionSource<Tuple<List<Counter>, ResponseStatus>>();

            lock (this.socket)
            {
                WriteBufferToStream(this.socket,
                    BitConverter.GetBytes((byte)0), 1);

                lock (this.callbacks["list"])
                {
                    this.callbacks["list"].Enqueue(socket => {
                        ResponseStatus status = (ResponseStatus)ReadStreamToBuffer(socket, buffer, 1)[0];

                        short count = BitConverter.ToInt16(
                            ReadStreamToBuffer(socket, buffer, 2), 0);

                        var counters = new List<Counter>(count);

                        for (short i = 0; i < count; i++)
                        {
                            counters.Add(ReadCounter());
                        }

                        promise.SetResult(Tuple.Create<List<Counter>, ResponseStatus>(counters, status));
                    });
                }
            }

            return promise.Task;
        }

        public Task<Tuple<Counter, ResponseStatus>> AddCounter(string category, string name)
        {
            var promise = new TaskCompletionSource<Tuple<Counter, ResponseStatus>>();

            lock (this.socket)
            {
                WriteBufferToStream(this.socket,
                    BitConverter.GetBytes((byte)1), 1);

                WriteBufferToStream(this.socket,
                    BitConverter.GetBytes(category.Length), 4);

                WriteBufferToStream(this.socket,
                    Encoding.Default.GetBytes(category), category.Length);

                WriteBufferToStream(this.socket,
                    BitConverter.GetBytes(name.Length), 4);

                WriteBufferToStream(this.socket,
                    Encoding.Default.GetBytes(name), name.Length);

                lock (this.callbacks["add"])
                {
                    this.callbacks["add"].Enqueue(socket => {
                        Counter counter = null;
                        ResponseStatus status = (ResponseStatus)ReadStreamToBuffer(socket, buffer, 1)[0];

                        if (status == ResponseStatus.OK)
                        {
                            counter = ReadCounter();

                            if (counters.ContainsKey(counter.Index))
                                throw new Exception();

                            this.counters.Add(counter.Index, counter);

                        }

                        promise.SetResult(Tuple.Create<Counter, ResponseStatus>(counter, status));
                    });
                }
            }

            return promise.Task;
        }

        public Task<ResponseStatus> RemoveCounters(short index)
        {
            var promise = new TaskCompletionSource<ResponseStatus>();

            lock (this.socket)
            {
                WriteBufferToStream(this.socket,
                    BitConverter.GetBytes((byte)2), 1);

                WriteBufferToStream(this.socket,
                    BitConverter.GetBytes(index), 2);

                lock (this.callbacks["remove"])
                {
                    this.callbacks["remove"].Enqueue(socket => {
                        promise.SetResult((ResponseStatus)ReadStreamToBuffer(socket, buffer, 1)[0]);
                    });
                }
            }

            return promise.Task;
        }

        public void Run()
        {
            listener.Start();

            while (Enable)
            {
                try
                {
                    socket = listener.AcceptSocket();
                    Debug.WriteLine("Connection open", "MonoCounters.Inspector.Run");

                    while (true)
                    {
                        byte cmd = ReadStreamToBuffer(socket, buffer, 1)[0];

                        Debug.WriteLine("Received command " + cmd.ToString(), "MonoCounters.Inspector.Run");

                        switch (cmd)
                        {
                            case 0: // Hello
                                short version = BitConverter.ToInt16(
                                                    ReadStreamToBuffer(socket, buffer, 2), 0);

                                short count = BitConverter.ToInt16(
                                                  ReadStreamToBuffer(socket, buffer, 2), 0);

                                for (short i = 0; i < count; i++)
                                {
                                    var counter = ReadCounter();

                                    this.counters.Add(counter.Index, counter);
                                }
                                break;
                            case 1: // List
                                lock (this.socket) {
                                    lock (this.callbacks["list"])
                                        this.callbacks["list"].Dequeue()(socket);
                                }
                                break;
                            case 2: // Add
                                lock (this.socket) {
                                    lock (this.callbacks["add"])
                                        this.callbacks["add"].Dequeue()(socket);
                                }
                                break;
                            case 3: // Remove
                                lock (this.socket) {
                                    lock (this.callbacks["remove"])
                                        this.callbacks["remove"].Dequeue()(socket);
                                }
                                break;
                            case 4: // Sampling
                                long timestamp = BitConverter.ToInt64(
                                    ReadStreamToBuffer(socket, buffer, 8), 0);

                                while (true)
                                {
                                    short index = BitConverter.ToInt16(
                                        ReadStreamToBuffer(socket, buffer, 2), 0);

                                    if (index < 0)
                                        break;

                                    Counter counter = new Counter(this.counters[index]);

                                    short size = BitConverter.ToInt16(
                                        ReadStreamToBuffer(socket, buffer, 2), 0);

                                    switch (counter.Type)
                                    {
                                        case Type.MONO_COUNTER_TYPE_INT:
                                            counter.Value = BitConverter.ToInt32(
                                                ReadStreamToBuffer(socket, buffer, 4), 0);
                                            break;
                                        case Type.MONO_COUNTER_TYPE_WORD:
                                            counter.Value = (size == 4) ?
                                                BitConverter.ToInt32(
                                                ReadStreamToBuffer(socket, buffer, 4), 0) :
                                                BitConverter.ToInt64(
                                                ReadStreamToBuffer(socket, buffer, 8), 0);
                                            break;
                                        case Type.MONO_COUNTER_TYPE_LONG:
                                            counter.Value = BitConverter.ToInt64(
                                                ReadStreamToBuffer(socket, buffer, 8), 0);
                                            break;
                                        case Type.MONO_COUNTER_TYPE_DOUBLE:
                                            counter.Value = BitConverter.ToDouble(
                                                ReadStreamToBuffer(socket, buffer, 8), 0);
                                            break;
                                    }

                                    this.counters[index] = counter;
                                }

                                if (SampleTick != null)
                                    SampleTick(this, new TickEventArgs(timestamp, new List<Counter>(this.counters.Values)));

                                break;
                        }
                    }
                }
                catch (SocketException e)
                {
                    Debug.WriteLine("Connection closed by the Agent. Exception : " + e.ToString(), "MonoCounters.Inspector.Run");
                }
                catch(ObjectDisposedException e)
                {
                    Debug.WriteLine("Connection closed by the Agent. Exception : " + e.ToString(), "MonoCounters.Inspector.Run");
                }
                finally
                {
                    if (socket != null)
                    {
                        Close();
                    }

                    this.counters.Clear();
                }

                Debug.WriteLine("Connection closed", "MonoCounters.Inspector");
            }

        }

        public void Close()
        {
            Debug.WriteLine("Closing connection", "MonoCounters.Inspector.Close");

            if (this.socket == null)
            {
                Debug.WriteLine("Socket was not open", "MonoCounters.Inspector.Close");
                return;
            }

            lock (this.socket)
            {
                if (socket.Connected)
                {
                    try {
                        Debug.WriteLine("Send command 127", "MonoCounters.Inspector.Close");
                        WriteBufferToStream(this.socket, new byte[] { 127 }, 1);
                    } finally {
                        socket.Close();
                    }
                }

            }
        }

        private static byte[] ReadStreamToBuffer(Socket socket, byte[] buffer, int size)
        {
            int total = 0, received;

            while (total < size)
            {
                received = socket.Receive(buffer, total, size - total, SocketFlags.None);

                if (received < 0)
                    throw new IOException();

                total += received;
            }

            return buffer;
        }

        private static byte[] WriteBufferToStream(Socket socket, byte[] buffer, int size)
        {
            int total = 0, sent;

            while (total < size)
            {
                sent = socket.Send(buffer, total, size - total, SocketFlags.None);

                if (sent < 0)
                    throw new IOException();

                total += sent;
            }

            return buffer;
        }

        private Counter ReadCounter()
        {
            var counter = new Counter();

            counter.Category = (Category)BitConverter.ToInt32(
                ReadStreamToBuffer(this.socket, this.buffer, 4), 0);

            int length = BitConverter.ToInt32(
                ReadStreamToBuffer(this.socket, this.buffer, 4), 0);

            if (length > this.buffer.Length)
                this.buffer = new byte[length];

            counter.Name = Encoding.Default.GetString(
                ReadStreamToBuffer(this.socket, this.buffer, length), 0, length);

            counter.Type = (Type)BitConverter.ToInt32(
                ReadStreamToBuffer(this.socket, this.buffer, 4), 0);

            counter.Unit = (Unit)BitConverter.ToInt32(
                ReadStreamToBuffer(this.socket, this.buffer, 4), 0);

            counter.Variance = (Variance)BitConverter.ToInt32(
                ReadStreamToBuffer(this.socket, this.buffer, 4), 0);

            counter.Index = BitConverter.ToInt16(
                ReadStreamToBuffer(this.socket, this.buffer, 2), 0);

            return counter;
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

            public TickEventArgs(long timestamp, List<Counter> counters)
            {
                Counters = counters;
                Timestamp = timestamp;
            }
        }
    }
}

