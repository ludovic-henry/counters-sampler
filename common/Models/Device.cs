using System;
using System.Data.Common;
using System.Data.Linq.Mapping;
using Mono.Data.Sqlite;

namespace MonoCounters.Web.Models
{
	[Table (Name = "devices")]
	public class Device : Base
	{
		[Column (Name = "manufacturer")]
		public string Manufacturer { get; set; }

		[Column (Name = "model")]
		public string Model { get; set; }

		[Column (Name = "cpu")]
		public string Cpu { get; set; }

		[Column (Name = "memory")]
		public string Memory { get; set; }

		protected Device (bool isNew) : base (isNew)
		{
		}

		public static void Initialize ()
		{
			new SqliteCommand (@"
				CREATE TABLE IF NOT EXISTS devices (
					id INTEGER PRIMARY KEY AUTOINCREMENT
				      , manufacturer TEXT NOT NULL
				      , model TEXT NOT NULL
				      , cpu TEXT NOT NULL
				      , memory TEXT NOT NULL
				);
			", Connection).ExecuteNonQuery ();
		}
	}
}
