using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using Mono.Data.Sqlite;
using Newtonsoft.Json;

namespace BenchmarkingSuite.Common.Models
{
	[Table (Name = "samples"), JsonObject(MemberSerialization.OptOut)]
	public class Sample : DatabaseModel
	{
		[Column (Name = "run_id"), JsonIgnoreAttribute] //, ForeignKey (ThisKey = "run_id", ForeignType = typeof (Run), ForeignKey = "id")]
		public long RunID { get; set; }

		[Column (Name = "counter_id"), JsonIgnoreAttribute] //, ForeignKey (ThisKey = "counter_id", ForeignType = typeof (Counter), ForeignKey = "id")]
		public long CounterID { get; set; }

		[Column (Name = "timestamp")]
		public ulong Timestamp { get; set; }

		[Column (Name = "value")]
		public object Value { get; set; }

		Run run = null;
		public Run Run {
			get { return run == null ? (run = IsConnectionOpen ? Run.FindByID (RunID) : null) : run; }
			set {
				RunID = value.ID;
				run = value;
			}
		}

		Counter counter = null;
		public Counter Counter {
			get { return counter == null ? (counter = IsConnectionOpen ? Counter.FindByID (CounterID) : null) : counter; }
			set {
				CounterID = value.ID;
				counter = value;
			}
		}

		static Sample ()
		{
			if (!IsConnectionOpen)
				return;

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

		public static List<Sample> All ()
		{
			return All<Sample> ();
		}

		public Sample Save ()
		{
			return Save<Sample> ();
		}

		public static Sample FindByID (long id)
		{
			return FindByID<Sample> (id);
		}
	}
}

