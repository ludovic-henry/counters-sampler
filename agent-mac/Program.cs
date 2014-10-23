using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using MonoCounters.Common.Agent;

using Newtonsoft.Json;

namespace MonoCounters.Agent.Mono
{
	public class MainClass
	{
		static string Coordinator = String.Empty;
		static string BenchmarkerDir = String.Empty;
		static string Architecture = String.Empty;
		static string Revision = String.Empty;
		static string Device = String.Empty;

		static void Usage ()
		{
			Console.Error.WriteLine ("usage: --coordinator <coordinator> --benchmarker-dir <benchmarker-dir> --architecture <architecture> --device <device> [--revision <revision>] [--recipe-ids <recipe-id>[,<recipe-id>]*]");
		}

		public static void Main (string[] args)
		{
			Coordinator = "127.0.0.1:8080";
			BenchmarkerDir = "/Users/ludovic/Xamarin/benchmarker";
			Architecture = "amd64";
			Device = "mac mini";
			Revision = "a68a79338360b04cd8a302154252b6e01c564a83"; //String.Empty;

			var recipe_ids = new int[0];

			for (var optindex = 1; optindex < args.Length; ++optindex) {
				if (args [optindex] == "--coordinator" || args [optindex].StartsWith ("--coordinator=")) {
					Coordinator = (args [optindex] == "--coordinator" ? args [++optindex] : args [optindex].Substring ("--coordinator=".Length)).Trim ();
				} else if (args [optindex] == "--benchmarker-dir" || args [optindex].StartsWith ("--benchmarker-dir=")) {
					BenchmarkerDir = (args [optindex] == "--benchmarker-dir" ? args [++optindex] : args [optindex].Substring ("--benchmarker-dir=".Length)).Trim ();
				} else if (args [optindex] == "--architecture" || args [optindex].StartsWith ("--architecture=")) {
					Architecture = (args [optindex] == "--architecture" ? args [++optindex] : args [optindex].Substring ("--architecture=".Length)).Trim ();
				} else if (args [optindex] == "--revision" || args [optindex].StartsWith ("--revision=")) {
					Revision = (args [optindex] == "--revision" ? args [++optindex] : args [optindex].Substring ("--revision=".Length)).Trim ();
				} else if (args [optindex] == "--recipe-ids" || args [optindex].StartsWith ("--recipe-ids=")) {
					recipe_ids = (args [optindex] == "--recipe-ids" ? args [++optindex] : args [optindex].Substring ("--recipe-ids=".Length))
						.Split (',').Select (s => s.Trim ()).Distinct ().Select (s => Int32.Parse (s)).ToArray ();
				} else if (args [optindex] == "--device" || args [optindex].StartsWith ("--device=")) {
					Device = (args [optindex] == "--device" ? args [++optindex] : args [optindex].Substring ("--device=".Length)).Trim ();
				} else {
					Console.Error.WriteLine ("unknown parameter {0}", args [optindex]);
					Usage ();
					Environment.Exit (1);
				}
			}

			if (String.IsNullOrEmpty (Coordinator)
				|| String.IsNullOrEmpty (BenchmarkerDir)
				|| String.IsNullOrEmpty (Architecture)
				|| String.IsNullOrEmpty (Device))
			{
				Usage ();
				Environment.Exit (1);
			}

			var archive_folder = FetchAndUnpackRevisionArchive ();
			var recipes = FetchRecipes (recipe_ids);

			foreach (var recipe in recipes) {
				RunRecipe (recipe, archive_folder);
			}

		}

		static string FetchAndUnpackRevisionArchive ()
		{
			if (String.IsNullOrEmpty (Revision)) {
				var revisions = JsonConvert.DeserializeObject<List<Models.Revision>> (GetHttpContent (String.Format (
							"http://{0}/revisions/mono/{1}", Coordinator, Architecture)));

				if (!revisions.Any ()) {
					Console.Error.WriteLine ("There is no revision for architecture {0}", Architecture);
					Environment.Exit (3);
				}

				Revision = revisions.OrderByDescending (r => r.CreationDate).First ().Commit;
			}

			var response = HttpWebRequest.CreateHttp (String.Format ("http://{0}/revision/mono/{1}/{2}.tar.gz", Coordinator, Architecture, Revision))
							.GetResponse ();

			var folder = Path.GetTempPath ();
			var filename = Path.GetTempFileName ();

			if (File.Exists (filename))
				File.Delete (filename);

			using (var file = new FileStream (filename, FileMode.Create, FileAccess.Write))
				response.GetResponseStream ().CopyTo (file);

			Console.Out.WriteLine ("[] Untar revision {0} to {1}", Revision, folder);

			var process = Process.Start (new ProcessStartInfo () {
				FileName = "tar",
				Arguments = String.Format ("xvzf {0}", filename),
				WorkingDirectory = folder,
				UseShellExecute = true,
			});

			process.WaitForExit ();

			return folder;
		}

		static List<KeyValuePair <int, Models.Recipe>> FetchRecipes (int[] ids)
		{
			var recipes = new List<KeyValuePair <int, Models.Recipe>> ();

			if (ids.Length == 0) {
				ids = JsonConvert.DeserializeObject <int[]> (GetHttpContent (String.Format (
					"http://{0}/recipes?device.architecture={1}&device.name={2}", Coordinator, Architecture, Device)));
			}

			Console.Out.WriteLine ("[] Fetch recipes {0}", String.Join (", ", ids));

			foreach (var id in ids) {
				recipes.Add (new KeyValuePair <int, Models.Recipe> (id, JsonConvert.DeserializeObject <Models.Recipe> (GetHttpContent (String.Format (
					"http://{0}/recipe/{1}", Coordinator, id)))));
			}

			return recipes;
		}

		static void RunRecipe (KeyValuePair<int, Models.Recipe> recipe, string archive_folder)
		{
			try {
				Console.Out.WriteLine ("[] Run recipe {0} with benchmark '{1}' on device '{2}'", recipe.Key, recipe.Value.Benchmark.Name, recipe.Value.Device.Name);

				var output = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName () + ".mlpd");

				var info = new ProcessStartInfo ();

				info.UseShellExecute = false;
				info.WorkingDirectory = Path.Combine (BenchmarkerDir, "tests");
				info.FileName =  Path.Combine (archive_folder, "mono");	
				info.Arguments = String.Join (" ", new string [] { recipe.Value.Configuration.Arguments, String.Format (
					"--profile=log:counters,nocalls,noalloc,output=-{0}", output), recipe.Value.Benchmark.Executable, recipe.Value.Benchmark.Arguments });

				info.EnvironmentVariables.Add ("MONO_PATH", archive_folder);
				info.EnvironmentVariables.Add ("LD_LIBRARY_PATH", archive_folder);

				foreach (var env in recipe.Value.Configuration.EnvironmentVariables.Split (' ')) {
					if (String.IsNullOrEmpty (env))
						continue;

					var a = env.Split (new char [] { '=' }, 2).Select (s => s.Trim ()).ToArray ();

					if (a [0] == "MONO_PATH" || a [0] == "LD_LIBRARY_PATH")
						continue;

					info.EnvironmentVariables.Add (a [0], a.Length > 1 ? a [1] : null);
				}

				Console.Out.WriteLine ("[] Execute benchmark : MONO_PATH={0} LD_LIBRARY_PATH={1} {2} {3}", archive_folder, archive_folder, info.FileName, info.Arguments);

				var start = DateTime.Now.Ticks;

				Process.Start (info).WaitForExit ();

				var end = DateTime.Now.Ticks;

				Console.Out.WriteLine ("[] Sending profiler output {0}", output);
				using (var file = new FileStream (output, FileMode.Open, FileAccess.Read))
					PostHttpContent (String.Format ("http://{0}/run?recipe_id={1}&start_date={2}&end_date={3}",
						Coordinator, recipe.Key, HttpUtility.UrlEncode (start.ToString ()), HttpUtility.UrlEncode (end.ToString ())), file);
			} catch (Exception e) {
				Console.WriteLine (e.ToString ());
			}
		}

		static string GetHttpContent (string url)
		{
			return new StreamReader (HttpWebRequest.CreateHttp (url).GetResponse ().GetResponseStream ()).ReadToEnd ();
		}

		static string PostHttpContent (string url, Stream content) {
			var request = HttpWebRequest.CreateHttp (url);
			request.Method = "POST";
			request.ContentType = "application/octet-stream";

			using (var s = request.GetRequestStream ())
				content.CopyTo (s);

			return new StreamReader (request.GetResponse ().GetResponseStream ()).ReadToEnd ();
		}
	}
}
