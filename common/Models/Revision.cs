using System;
using System.Data.Common;
using System.Data.Linq.Mapping;
using Mono.Data.Sqlite;

namespace MonoCounters.Web.Models
{
	[Table (Name = "revisions")]
	public class Revision : Base
	{
		[Column (Name = "project_id"), ForeignKey (ThisKey = "project_id", ForeignType = typeof (Project), ForeignKey = "id")]
		public Project Project { get; set; }

		[Column (Name = "run_id"), ForeignKey (ThisKey = "run_id", ForeignType = typeof (Run), ForeignKey = "id")]
		public Run Run { get; set; }

		[Column (Name = "sha")]
		public string Sha { get; set; }

		protected Revision (bool isNew) : base (isNew)
		{
		}

		public static void Initialize ()
		{
			new SqliteCommand (@"
				CREATE TABLE IF NOT EXISTS revisions (
					id INTEGER PRIMARY KEY AUTOINCREMENT
				      , project_id INTEGER REFERENCES projects (id) ON DELETE CASCADE ON UPDATE CASCADE
				      , run_id INTEGER REFERENCES runs (id) ON DELETE CASCADE ON UPDATE CASCADE
				      , sha TEXT NOT NULL
				      , UNIQUE (project_id, sha)
				);
			", Connection).ExecuteNonQuery ();
		}
	}
}
