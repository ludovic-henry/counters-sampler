using System;
using Nancy;
using Nancy.TinyIoc;

namespace MonoCounters.Web
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        protected override IRootPathProvider RootPathProvider
        {
            get { return new CustomRootPathProvider(); }
        }
    }

    public class CustomRootPathProvider : IRootPathProvider
    {
        public string GetRootPath()
        {
            return "/Users/ludovic/Xamarin/counters-sampler/manager-web";
        }
    }
}

