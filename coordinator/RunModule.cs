using System;
using System.Collections.Generic;
using System.IO;

using MonoCounters.Models;

using Nancy;
using Nancy.Responses;

namespace MonoCounters.Coordinator
{
	public class RunModule : NancyModule
	{
		public RunModule ()
		{
			Post ["/run"] = parameters => {
				if (!Request.Query ["recipe_id"].HasValue
					|| !Request.Query ["start_date"].HasValue
					|| !Request.Query ["end_date"].HasValue)
					return new TextResponse (HttpStatusCode.BadRequest);

				var recipe_id = Int64.Parse (Request.Query ["recipe_id"]);
				var start_date = new DateTime (Int64.Parse (Request.Query ["start_date"]));
				var end_date = new DateTime (Int64.Parse (Request.Query ["end_date"]));

				var filename = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName () + ".mlpd");

				Console.Out.WriteLine ("[] RunModule : recipe_id {0} start_date {1} end_date {2} duration {3}ms body.length {4} output {5}", recipe_id, start_date, end_date, (end_date - start_date).ToString (), Request.Body.Length, filename);

				using (var output = new FileStream (filename, FileMode.Create, FileAccess.Write, FileShare.Read)) {
					Request.Body.CopyTo (output);

					var info = new FileInfo (filename);
					if (info.Length == 0)
						return new TextResponse (HttpStatusCode.BadRequest, "Empty profiler output");

					if (Request.Body.Length != output.Length)
						throw new Exception ("body.Length != output.Length");

					var run = new Run () { RecipeID = recipe_id, StartDate = start_date, EndDate = end_date }.Save ();

					var inspector = new Common.Inspector.Inspector (filename);
					var cache = new Dictionary<ulong, Models.Counter> ();

					inspector.UpdatedSample += (sender, e) => {
						var timestamp = e.Timestamp;

						foreach (var counter in e.Counters) {
							if (!cache.ContainsKey (counter.Index)) {
								var c = Models.Counter.FindByCategoryAndName (counter.Category, counter.Name);
								if (c == null) {
									c = new Models.Counter () { Category = counter.Category, Name = counter.Name, Type = counter.TypeName,
										Unit = counter.UnitName, Variance = counter.VarianceName};

									c.Save ();
								}

								cache.Add (counter.Index, c);
							}

							new Sample () { RunID = run.ID, CounterID = cache [counter.Index].ID, Timestamp = timestamp, Value = counter.Value }.Save ();
						}
					};

					inspector.Run ();
				}

				return Response.AsJson ("");
			};

			OnError += (ctx, e) => {
				Console.WriteLine (e.ToString ());

				return new TextResponse (HttpStatusCode.InternalServerError, e.ToString ());
			};
		}
	}
}

