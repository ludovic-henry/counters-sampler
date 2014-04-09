using System;
using System.Data.Linq.Mapping;
using Mono.Data.Sqlite;

namespace MonoCounters.Models
{
	[Table (Name = "recipes")]
	public class Recipe : Base
	{
		[Column (Name = "benchmark_id"), ForeignKey (ThisKey = "benchmark_id", ForeignType = typeof (Benchmark), ForeignKey = "id")]
		public Benchmark Benchmark { get; set; }

		[Column (Name = "configuration_id"), ForeignKey (ThisKey = "configuration_id", ForeignType = typeof (Configuration), ForeignKey = "id")]
		public Configuration Configuration { get; set; }

		[Column (Name = "device_id"), ForeignKey (ThisKey = "device_id", ForeignType = typeof (Device), ForeignKey = "id")]
		public Device Device { get; set; }

		protected Recipe (bool isNew) : base (isNew)
		{
		}

		public static void Initialize ()
		{
			new SqliteCommand (@"
				CREATE TABLE IF NOT EXISTS recipes (
					id INTEGER PRIMARY KEY AUTOINCREMENT
				      , benchmark_id INTEGER REFERENCES benchmarks (id) ON DELETE CASCADE ON UPDATE CASCADE
				      , configuration_id INTEGER REFERENCES configurations (id) ON DELETE CASCADE ON UPDATE CASCADE
				      , device_id INTEGER REFERENCES devices (id) ON DELETE CASCADE ON UPDATE CASCADE
				);
			", Connection).ExecuteNonQuery ();
		}
	}
}

