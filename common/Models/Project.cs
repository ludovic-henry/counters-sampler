using System;
using System.Data.Common;
using System.Data.Linq.Mapping;
using Mono.Data.Sqlite;

namespace MonoCounters.Web.Models
{
	[Table (Name = "projects")]
	public class Project : Base
	{
		[Column (Name = "owner")]
		public string Owner { get; set; }

		[Column (Name = "repo")]
		public string Repo { get; set; }

		public static void Initialize ()
		{
			new SqliteCommand (@"
				CREATE TABLE IF NOT EXISTS projects (
					id INTEGER PRIMARY KEY AUTOINCREMENT
				      , owner TEXT NOT NULL
				      , repo TEXT NOT NULL
				);
			", Connection).ExecuteNonQuery ();
		}
	}
}

