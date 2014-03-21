using System;
using System.Collections.Generic;


namespace MonoCounters
{
    public class History
    {
        private SortedDictionary<long, Dictionary<short, Counter>> history;
        private Inspector inspector;

        public History(Inspector inspector)
        {
            this.history = new SortedDictionary<long, Dictionary<short, Counter>>();

            this.inspector = inspector;
            this.inspector.SampleTick += OnInspectorSampleTick;
        }

        public SortedDictionary<long, List<Counter>> this[long since, long limit]
        {
            get
            {
                SortedDictionary<long, List<Counter>> updated = new SortedDictionary<long, List<Counter>>();

                lock (this.history)
                {
                    long first = 0;

                    foreach (var e in this.history)
                    {
                        if (e.Key > since)
                        {
                            if (first == 0)
                                first = e.Key;
                            else if (e.Key > first + limit)
                                break;

                            updated.Add(e.Key, new List<Counter>(e.Value.Values));
                        }
                    }
                }

                return updated;
            }
        }

        public long LastTimestamp
        {
            get
            {
                long last = 0;

                lock (this.history)
                {
                    foreach (var e in this.history)
                    {
                        if (e.Key > last)
                        {
                            last = e.Key;
                        }
                    }
                }

                return last;
            }
        }

        public void Clear()
        {
            this.history.Clear();
        }

        private void OnInspectorSampleTick (object sender, Inspector.TickEventArgs a) 
        {
            Dictionary<short, Counter> counters;

            if (this.history.Count == 0)
            {
                counters = new Dictionary<short, Counter>();

                foreach (var c in a.Counters)
                    counters.Add(c.Index, c);
            }
            else
            {
                counters = new Dictionary<short, Counter>(this.history[LastTimestamp]);

                foreach (var c in a.Counters)
                    counters[c.Index] = c;
            }

            lock (this.history)
            {
                this.history.Add(a.Timestamp, counters);
            }
        }
    }
}

