using System;
using System.IO;
using System.Reflection;
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
			return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().FullName), "..", "..");
		}
	}
}
