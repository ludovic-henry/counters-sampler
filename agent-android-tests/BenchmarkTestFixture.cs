using System;

using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using MonoCounters.Common.Agent;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace agentandroidtests
{
	[TestFixture]
	public class BenchmarkTestFixture
	{
		AndroidApp app;

		string AppFile = "agent_android.agent_android-Signed.apk";

		[SetUp]
		public void SetUp ()
		{
			app = ConfigureApp
				.Android
				.ApkFile (AppFile)
				.StartApp ();
		}

		[Test]
		public void WaitForNoBenchmarkImageTest ()
		{
			app.WaitForNoElement (c => c.Id ("BenchmarkImage"));
		}
	}
}

