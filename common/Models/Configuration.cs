using System;
using System.Data.Linq.Mapping;
using Mono.Data.Sqlite;

namespace MonoCounters.Models
{
	[Table(Name = "configurations")]
	public class Configuration : Base
	{
		[Column(Name = "mono_runtime")]
		public string MonoRuntime { get; set; }

		[Column(Name = "mono_arguments")]
		public string MonoArguments { get; set; }

		[Column(Name = "environment_variables")]
		public string EnvironmentVariables { get; set; }

		protected Configuration (bool isNew) : base (isNew)
		{
		}

		public static void Initialize ()
		{
			new SqliteCommand (@"
				CREATE TABLE IF NOT EXISTS configurations (
					id INTEGER PRIMARY KEY AUTOINCREMENT
				      , mono_runtime TEXT NOT NULL
				      , mono_arguments TEXT NOT NULL
				      , environment_variables TEXT NOT NULL
				);
			", Connection).ExecuteNonQuery();
		}
    }
}

