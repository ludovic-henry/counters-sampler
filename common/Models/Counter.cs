using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Threading.Tasks;
using Mono.Data.Sqlite;

namespace BenchmarkingSuite.Common.Models
{
	[Table (Name = "counters")]
	public class Counter : DatabaseModel
	{
		[Column (Name = "section")]
		public string Section { get; set; }

		[Column (Name = "name")]
		public string Name { get; set; }

		[Column (Name = "type")]
		public ulong Type { get; set; }

		[Column (Name = "unit")]
		public ulong Unit { get; set; }

		[Column (Name = "variance")]
		public ulong Variance { get; set; }

		static Counter ()
		{
			if (!IsConnectionOpen)
				return;

			new SqliteCommand (@"
				CREATE TABLE IF NOT EXISTS counters (
					id INTEGER PRIMARY KEY AUTOINCREMENT
				      , section TEXT NOT NULL
				      , name TEXT NOT NULL
				      , type INTEGER NOT NULL
				      , unit INTEGER NOT NULL
				      , variance INTEGER NOT NULL
				);
				CREATE UNIQUE INDEX IF NOT EXISTS counters_category_name_unique ON counters (category, name);
			", Connection).ExecuteNonQuery ();
		}

		public Counter () : base ()
		{
		}

		protected Counter (bool isNew) : base (isNew)
		{
		}

		public Counter Save ()
		{
			return Save<Counter> ();
		}

		public static List<Counter> All ()
		{
			return All<Counter> ();
		}

		public static Counter FindByID (long id)
		{
			return FindByID<Counter> (id);
		}

		public static Counter FindByCategoryAndName (string category, string name)
		{
			return FindBy<Counter> (new SortedDictionary<string, object> () { { "category", category }, { "name", name } }).FirstOrDefault ();
		}
	}
}

