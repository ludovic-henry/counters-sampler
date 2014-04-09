using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using Mono.Data.Sqlite;

namespace MonoCounters.Models
{
	[Table (Name = "runs")]
	public class Run : Base
	{
		[Column (Name = "recipe_id"), ForeignKey (ThisKey = "recipe_id", ForeignType = typeof (Recipe), ForeignKey = "id")]
		public Recipe Recipe { get; set; }

		[Column (Name = "start_date")]
		public DateTime StartDate { get; set; }

		[Column (Name = "end_date")]
		public DateTime EndDate { get; set; }

		public List<Revision> Revisions {
			get {
				var revisions = new List<Revision> ();

				var sql = new SqliteCommand ("SELECT revision_id FROM dependencies WHERE run_id = ?", Connection);

				sql.Parameters.Add (new SqliteParameter () { Value = ID });

				using (var reader = sql.ExecuteReader ()) {
					while (reader.Read ()) {
						revisions.Add (Revision.FindByID<Revision> (reader.GetInt64 (0)));
					}
				}

				return revisions;
			}
		}

		public static void Initialize ()
		{
			new SqliteCommand (@"
				CREATE TABLE IF NOT EXISTS runs (
					id INTEGER PRIMARY KEY AUTOINCREMENT
				      , recipe_id INTEGER REFERENCES recipes (id) ON DELETE CASCADE ON UPDATE CASCADE
				      , start_date DATETIME NOT NULL
				      , end_date DATETIME NOT NULL
				);
			", Connection).ExecuteNonQuery ();
		}
	}
}

