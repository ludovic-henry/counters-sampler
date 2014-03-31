using System;
using System.Reflection;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;
using System.Text;

namespace MonoCounters.Agent.Android
{
	[Activity (Label = "agent-android", MainLauncher = true)]
	public class MainActivity : Activity
	{

		string assembly = "benchmark.exe";
		string monoExe = "mono-sgen";

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			var mono = OpenFileOutput ("mono", FileCreationMode.Private);
			var benchmark = OpenFileOutput ("benchmark.exe", FileCreationMode.Private);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			var p = ApplicationContext.FilesDir.AbsolutePath;

			Assets.Open (monoExe).CopyTo (mono);
			Assets.Open (assembly).CopyTo (benchmark);

			var files = Directory.GetFiles (ApplicationContext.FilesDir.AbsolutePath);

			var m = new FileInfo (ApplicationContext.FilesDir.AbsolutePath + "/mono");
			var b = new FileInfo (ApplicationContext.FilesDir.AbsolutePath + "/benchmark.exe");

			var runtime = Java.Lang.Runtime.GetRuntime ();

			Console.WriteLine (Exec (runtime, new string [] { "/system/bin/chmod", "744", ApplicationContext.FilesDir.AbsolutePath + "/mono" }));

//			Console.WriteLine ("before {0}", ApplicationContext.FilesDir.AbsolutePath);
//			Console.WriteLine (Exec (runtime, new string [] { "ls", "-alh", ApplicationContext.FilesDir.AbsolutePath }));
//			Console.WriteLine ("after");

			Console.WriteLine (Exec (runtime, new string [] {
				ApplicationContext.FilesDir.AbsolutePath + "/mono", ApplicationContext.FilesDir.AbsolutePath + "/benchmark.exe"
			}));
		}

		protected string Exec (Java.Lang.Runtime runtime, string [] command) {
			return new StreamReader (runtime.Exec (command).InputStream).ReadToEnd ();
		}
	}
}


