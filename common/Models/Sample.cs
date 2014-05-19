using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using Mono.Data.Sqlite;

namespace MonoCounters.Models
{
	[Table (Name = "samples")]
	public class Sample : Base
	{
		[Column (Name = "run_id"), ForeignKey (ThisKey = "run_id", ForeignType = typeof (Run), ForeignKey = "id")]
		public Run Run { get; set; }

		[Column (Name = "counter_id"), ForeignKey (ThisKey = "counter_id", ForeignType = typeof (Counter), ForeignKey = "id")]
		public Counter Counter { get; set; }

		[Column (Name = "timestamp", DbType = "integer")]
		public ulong Timestamp { get; set; }

		[Column (Name = "value")]
		public object Value { get; set; }

		public static void Initialize ()
		{
			new SqliteCommand (@"
				CREATE TABLE IF NOT EXISTS samples (
					id INTEGER PRIMARY KEY AUTOINCREMENT
				      , run_id INTEGER REFERENCES runs (id) ON DELETE CASCADE ON UPDATE CASCADE
				      , counter_id INTEGER REFERENCES counters (id) ON DELETE CASCADE ON UPDATE CASCADE
				      , timestamp INTEGER NOT NULL
				      , value NUMERIC NOT NULL
				      , UNIQUE (run_id, counter_id, timestamp)
				);
			", Connection).ExecuteNonQuery ();
		}
	}
}

