using System;
using Nancy.Json;
using System.Collections.Generic;

namespace MonoCounters.Web
{
    public class CountersModule : Nancy.NancyModule
    {
        public CountersModule(History history)
        {
            var serializer = new JavaScriptSerializer();

            Get["/"] = parameters =>
            {
                return View["Index"];
            };

            Get["/counters"] = parameters =>
            {
                var counters = new SortedDictionary<string, List<Counter>>();

//                var after = parameters.after != null ? long.Parse(parameters.after) : 0L;
//                var limit = parameters.limit != null ? long.Parse(parameters.limit) : long.MaxValue;
//
//                foreach (var e in history.GetUpdatedSince(after, limit))
//                {
//                    counters.Add(e.Key.ToString(), e.Value);
//                }

                return serializer.Serialize(counters);
            };
        }
    }
}

