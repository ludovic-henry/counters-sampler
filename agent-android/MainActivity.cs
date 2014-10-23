using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MonoCounters.Agent;
using MonoCounters.Common.Agent;

namespace MonoCounters.Agent.Android
{
	[Activity (MainLauncher = true)]
	public class MainActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Main);

			var task = Task.Run (() => {
				try {
					// FIXME
					var inspector = "10.1.12.185:8080";
					var recipeId = 1;

					var output = "/sdcard/benchmark-profile.mpld";

					var versions = new Dictionary<string, string> {
						{ "mono", Mono.Runtime.GetDisplayName () },
						{ "monodroid", "" }
					};

					Console.WriteLine ("MainActivity | Benchmark : start");

					var args = Arguments.Parse (Assets.Open ("arguments"));

					await Assets.Open (args.Assembly).CopyToAsync (
						OpenFileOutput ("benchmark.exe", FileCreationMode.WorldReadable));

					var time = new Benchmark (Assembly.LoadFile (FilesDir.AbsolutePath + "/benchmark.exe"), args.Class)
						.Run (args.BenchmarkArguments.ToArray ());

					Console.WriteLine ("MainActivity | Benchmark : end, time = {0:N3} ms", time / 10000d);
					Console.WriteLine ("MainActivity | Log Upload : start, inspector = {3}, recipeId = {0}, output = {1}, versions = {2}",
								recipeId, output, versions, inspector);

					new ResultUploader (inspector).Upload (recipeId, new FileStream (output, FileMode.Open));

					Console.WriteLine ("MainActivity | Log Upload : end");
				} catch (Exception e) {
					Console.WriteLine (e);
				} finally {
					Finish ();
				}
			});

			task.Start ();
		}
	}
}

