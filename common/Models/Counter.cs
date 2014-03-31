using System;
using System.Data.Linq.Mapping;
using Mono.Data.Sqlite;

namespace MonoCounters.Web.Models
{
	[Table (Name = "counters")]
	public class Counter : Base
	{
		[Column (Name = "category")]
		public string Category { get; set; }

		[Column (Name = "name")]
		public string Name { get; set; }

		[Column (Name = "type")]
		public string Type { get; set; }

		[Column (Name = "unit")]
		public string Unit { get; set; }

		[Column (Name = "variance")]
		public string Variance { get; set; }

		public Counter () : base ()
		{
		}

		protected Counter (bool isNew) : base (isNew)
		{
		}

		public static void Initialize ()
		{
			new SqliteCommand (@"
				CREATE TABLE IF NOT EXISTS counters (
					id INTEGER PRIMARY KEY AUTOINCREMENT
				      , category TEXT NOT NULL
				      , name TEXT NOT NULL
				      , type TEXT NOT NULL
				      , unit TEXT NOT NULL
				      , variance TEXT NOT NULL
				);
				CREATE UNIQUE INDEX IF NOT EXISTS counters_category_name_unique ON counters (category, name);
			", Connection).ExecuteNonQuery ();
		}

		public static Counter FindByCategoryAndName (string category, string name)
		{
			var sql = new SqliteCommand ("SELECT id, category, name, type, unit, variance FROM counters WHERE category = ? AND name = ?", Connection);

			sql.Parameters.Add (new SqliteParameter () { Value = category });
			sql.Parameters.Add (new SqliteParameter () { Value = name });

			using (var reader = sql.ExecuteReader ()) {
				while (reader.Read ()) {
					return new Counter (false) { ID = reader.GetInt64 (0), Category = reader.GetString (1), Name = reader.GetString (2), 
						Type = reader.GetString (3), Unit = reader.GetString (4), Variance = reader.GetString (5) }; 
				}
			}

			return null;
		}
	}
}

