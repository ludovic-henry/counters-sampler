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
            this.inspector.Tick += OnInspectorTick;
        }

        public SortedDictionary<long, List<Counter>> GetUpdatedSince(long timestamp, long limit)
        {
            SortedDictionary<long, List<Counter>> updated = new SortedDictionary<long, List<Counter>>();

            lock (this.history)
            {
                long first = -1;

                foreach (var e in this.history)
                {
                    if (e.Key > timestamp)
                    {
                        if (first < 0)
                            first = e.Key;
                        else if (e.Key > first + limit)
                            break;

                        updated.Add(e.Key, new List<Counter>(e.Value.Values));
                    }
                }
            }

            return updated;
        }

        private void OnInspectorTick (object sender, Inspector.TickEventArgs a) 
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
                var last = 0L;

                foreach (var k in this.history.Keys)
                    if (k > last)
                        last = k;

                counters = new Dictionary<short, Counter>(this.history[last]);

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

