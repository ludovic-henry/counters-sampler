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

                var counters = new List<object>();

                foreach (var t in result) {
                    counters.Add(new { category = t.Item1, name = t.Item2 });
                }

                return Response.AsJson(counters);
            };

            Post["/counters", true] = async (parameters, ct) =>
            {
                if (!Request.Form["category"].HasValue || !Request.Form["name"].HasValue)
                    return HttpStatusCode.BadRequest;

                var result = await InspectorModel.Inspector.AddCounter(Request.Form["category"], Request.Form["name"]);

                var counter = (Counter) result.Item1;
                var status = (Inspector.ResponseStatus) result.Item2;

                return Response.AsJson(new { status = status, counter = new {
                        category = counter.CategoryName,
                        name = counter.Name,
                        type = counter.TypeName,
                        unit = counter.UnitName,
                        variance = counter.VarianceName,
                        value = counter.Value,
                        index = counter.Index
                    } });
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
                var counters = new SortedDictionary<string, List<object>>();

                var since = Request.Query["since"].HasValue ? long.Parse(Request.Query["since"]) : 0L;
                var limit = Request.Query["limit"].HasValue ? long.Parse(Request.Query["limit"]) : long.MaxValue;

                foreach (var e in HistoryModel.History[since, limit])
                {
                    var list = new List<object>();

                    foreach (var counter in e.Value)
                    {
                        list.Add(new {
                            category = counter.CategoryName,
                            name = counter.Name,
                            type = counter.TypeName,
                            unit = counter.UnitName,
                            variance = counter.VarianceName,
                            value = counter.Value,
                            index = counter.Index
                        });
                    }

                    counters.Add(e.Key.ToString(), list);
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

