using System;
using Nancy;

namespace MonoCounters.Web
{
    public class StaticModule : NancyModule
    {
        public StaticModule()
        {
            Get["/"] = parameters =>
            {
                return View["index"];
            };
        }
    }
}

