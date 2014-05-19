using System;
using System.Collections.Generic;
using MonoCounters.Models;
using Nancy;
using System.IO;

namespace MonoCounters.Inspector
{
	public class RunModule : NancyModule
	{
		public RunModule ()
		{
			Post ["/run"] = parameters => {
				if (!Request.Form ["recipe_id"].HasValue)
					return Response.AsJson ("", HttpStatusCode.BadRequest);
				try {
					var recipe = Recipe.FindByID<Recipe> (Request.Form ["recipe_id"]);
					var run = new Run () { Recipe = recipe }.Save<Run> ();

					foreach (var file in Request.Files) {
						if (file.Key.Equals ("samples")) {
							var filename = Path.GetTempFileName ();
							using (var output = new FileStream (filename, FileMode.OpenOrCreate | FileMode.Truncate, FileAccess.Write, FileShare.Read))
								file.Value.CopyTo (output);

							var inspector = new Common.Inspector.Inspector (filename);
							var cache = new Dictionary<ulong, Models.Counter> ();

							inspector.UpdatedSample += (sender, e) => {
								var timestamp = e.Timestamp;

								foreach (var counter in e.Counters) {
									if (!cache.ContainsKey (counter.Index)) {
										cache.Add (counter.Index,
											Models.Counter.FindByCategoryAndName (counter.CategoryName, counter.Name) ??
												new Models.Counter () { 
													Category = counter.CategoryName, Name = counter.Name, Type = counter.TypeName, 
													Unit = counter.UnitName, Variance = counter.VarianceName
												}.Save<Models.Counter> ());
									}

									new Sample () { Run = run, Counter = cache [counter.Index], Timestamp = timestamp, Value = counter.Value }
										.Save<Sample> ();
								}
							};

							inspector.Run ();
						}
					}
				} catch (Exception e) {
					Console.WriteLine (e);
				}

				return Response.AsJson ("");
			};
		}
	}
}

