using System;
using System.Data.Common;
using System.Data.Linq.Mapping;
using Mono.Data.Sqlite;

namespace MonoCounters.Web.Models
{
	[Table (Name = "benchmarks")]
	public class Benchmark : Base
	{
		[Column (Name = "name")]
		public string Name { get; set; }

		[Column (Name = "exe_name")]
		public string ExeName { get; set; }

		[Column (Name = "working_directory")]
		public string WorkingDirectory { get; set; }

		[Column (Name = "arguments")]
		public string Arguments { get; set; }

		protected Benchmark (bool isNew) : base (isNew)
		{
		}

		public static void Initialize ()
		{
			new SqliteCommand (@"
				CREATE TABLE IF NOT EXISTS benchmarks (
					id INTEGER PRIMARY KEY AUTOINCREMENT
				      , name TEXT NOT NULL
				      , exe_name TEXT NOT NULL
				      , working_directory TEXT NOT NULL
				      , arguments TEXT NOT NULL
				);
			", Connection).ExecuteNonQuery ();
		}
	}
}
