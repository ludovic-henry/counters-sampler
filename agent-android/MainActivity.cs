using System;
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

namespace MonoCounters.Agent.Android
{
	[Activity (MainLauncher = true)]
	public class MainActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Main);

			var task = new Task (async () => {
				try {
					Console.WriteLine ("MainActivity | Benchmark : start");

					var args = Arguments.Parse (Assets.Open ("arguments.ini"));

					await Assets.Open (args.Assembly).CopyToAsync (
						OpenFileOutput ("benchmark.exe", FileCreationMode.WorldReadable));

					var time = new Benchmark (Assembly.LoadFile (FilesDir.AbsolutePath + "/benchmark.exe"), args.Class)
						.Run (args.BenchmarkArguments.ToArray ());

					Console.WriteLine ("MainActivity | Benchmark : end, time = {0:N3} ms", time / 10000d);
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

