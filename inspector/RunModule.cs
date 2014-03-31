using System;
using System.Collections.Generic;
using Nancy;

namespace MonoCounters.Web.Models
{
	public class RunModule : NancyModule
	{
		public RunModule ()
		{
			Post ["/run"] = parameters => {
				if (!Request.Form ["recipe_id"].HasValue)
					return Response.AsJson ("", HttpStatusCode.BadRequest);

				var recipe = Recipe.FindByID<Recipe> (Request.Form ["recipe_id"]);
				var run = new Run () { Recipe = recipe }.Save<Run> ();

				foreach (var file in Request.Files) {
					if (file.Key.Equals ("samples")) {
						var inspector = new Inspector (file.Value);
						var cache = new Dictionary<short, Counter> ();

						inspector.SampleTick += delegate(object sender, Inspector.TickEventArgs e) {
							var timestamp = e.Timestamp;
							var counters = e.Counters;

							foreach (var counter in counters) {
								if (!cache.ContainsKey (counter.Index)) {
									cache.Add (counter.Index,
										Counter.FindByCategoryAndName (counter.CategoryName, counter.Name) ?? new Counter () { 
											Category = counter.CategoryName, Name = counter.Name, Type = counter.TypeName, 
											Unit = counter.UnitName, Variance = counter.VarianceName }.Save<Counter> ());
								}

								new Sample () { Run = run, Counter = cache [counter.Index], Timestamp = timestamp, Value = counter.Value }
									.Save<Sample> ();
							}
						};

						inspector.Run ();
					}
				}

				return Response.AsJson ("");
			};
		}
	}
}

