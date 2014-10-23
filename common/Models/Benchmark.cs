using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using Mono.Data.Sqlite;

namespace BenchmarkingSuite.Common.Models
{
	[Table (Name = "benchmarks")]
	public class Benchmark : DatabaseModel
	{
		[Column (Name = "name")]
		public string Name { get; set; }

		[Column (Name = "executable")]
		public string Executable { get; set; }

		[Column (Name = "arguments")]
		public string Arguments { get; set; }

		static Benchmark ()
		{
			if (!IsConnectionOpen)
				return;

			new SqliteCommand (@"
				CREATE TABLE IF NOT EXISTS benchmarks (
					id INTEGER PRIMARY KEY AUTOINCREMENT
				      , name TEXT NOT NULL
				      , executable TEXT NOT NULL
				      , arguments TEXT NOT NULL
				      , UNIQUE (name)
				);
			", Connection).ExecuteNonQuery ();
		}

		public Benchmark () : base ()
		{
		}

		internal Benchmark (bool isNew) : base (isNew)
		{
		}

		public Benchmark Save (bool ignore = false)
		{
			return Save<Benchmark> (ignore);
		}

		public static List<Benchmark> All ()
		{
			return All<Benchmark> ();
		}

		public static Benchmark FindByID (long id)
		{
			return FindByID<Benchmark> (id);
		}

		public static Benchmark FindByName (string name)
		{
			return FindBy<Benchmark> (new SortedDictionary<string, object> () { { "name", name } }).FirstOrDefault ();
		}
	}
}
