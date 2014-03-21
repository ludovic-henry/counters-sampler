using System;
using System.Collections.Generic;
using Nancy;
using Nancy.Json;
using Nancy.TinyIoc;
using MonoCounters.Web.Models;
using Mono.Attach;

namespace MonoCounters.Web
{

    public class CountersModule : NancyModule
    {
        public CountersModule()
        {
            Get["/"] = parameters =>
            {
                return View["index"];
            };

            Get["/counters", true] = async (parameters, ct) =>
            {
                var result = await InspectorModel.Inspector.ListCounters();

                var counters = (List<Counter>) result.Item1;
                var status = (Inspector.ResponseStatus) result.Item2;

                return Response.AsJson(new { counters = counters, status = status });
            };

            Post["/counters", true] = async (parameters, ct) =>
            {
                if (!Request.Form["category"].HasValue || !Request.Form["name"].HasValue)
                    return HttpStatusCode.BadRequest;

                var result = await InspectorModel.Inspector.AddCounter(Request.Form["category"], Request.Form["name"]);

                var counter = (Counter) result.Item1;
                var status = (Inspector.ResponseStatus) result.Item2;

                return Response.AsJson(new { counter = counter, status = status });
            };

            Delete["/counters", true] = async (parameters, ctor) =>
            {
                if (!Request.Form["index"].HasValue)
                    return HttpStatusCode.BadRequest;

                var result = await InspectorModel.Inspector.RemoveCounters(Request.Form["index"]);

                var status = (Inspector.ResponseStatus) result;
   
                return Response.AsJson(new { status = status });
            };

            Get["/counters/history"] = parameters =>
            {
                var counters = new SortedDictionary<string, List<Counter>>();

                var since = Request.Query["since"].HasValue ? long.Parse(Request.Query["since"]) : 0L;
                var limit = Request.Query["limit"].HasValue ? long.Parse(Request.Query["limit"]) : long.MaxValue;

                foreach (var e in HistoryModel.History[since, limit])
                {
                    counters.Add(e.Key.ToString(), e.Value);
                }

                return Response.AsJson(counters);
            };

            Get["/counters/last/timestamp"] = parameters =>
            {
                return Response.AsJson(HistoryModel.History.LastTimestamp);
            };
        }
    }
}

