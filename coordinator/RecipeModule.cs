using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Nancy;
using Nancy.Responses;
using Newtonsoft.Json;

namespace MonoCounters.Coordinator
{
	public class RecipeModule : NancyModule
	{
		public RecipeModule ()
		{
			Get ["/recipes"] = (parameters) => {
				var device_name = Request.Query ["device.name"];
				var device_architecture = Request.Query ["device.architecture"];
				var benchmark_name = Request.Query ["benchmark.name"];

				List<Models.Device> devices;

				if (device_name.HasValue && device_architecture.HasValue)
					devices = new Models.Device [] { Models.Device.FindByNameAndArchitecture (device_name.Value, device_architecture.Value) }
							.Where (d => d != null).ToList ();
				else if (device_name.HasValue)
					devices = Models.Device.FilterByName (device_name.Value);
				else if (device_architecture.HasValue)
					devices = Models.Device.FilterByArchitecture (device_architecture.Value);
				else
					devices = Models.Device.All ();

				List<Models.Benchmark> benchmarks;

				if (benchmark_name.HasValue)
					benchmarks = new Models.Benchmark [] { Models.Benchmark.FindByName (benchmark_name.Value) }
							.Where (b => b != null).ToList ();
				else
					benchmarks = Models.Benchmark.All ();

				return new TextResponse (HttpStatusCode.OK, JsonConvert.SerializeObject (
						Models.Recipe.FilterByDevicesAndBenchmarks (devices, benchmarks).Select (r => r.ID)));
			};

			Get ["/recipe/{id}"] = (parameters) => {
				var recipe = Models.Recipe.FindByID (parameters.id);

				if (recipe == null)
					return new TextResponse (HttpStatusCode.NotFound, "Revision not found");

				return new TextResponse (HttpStatusCode.OK, JsonConvert.SerializeObject (recipe));
			};

			OnError += (ctx, e) => {
				return new TextResponse (HttpStatusCode.InternalServerError, e.ToString ());
			};
		}
	}
}

