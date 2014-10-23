using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using Newtonsoft.Json;

namespace BenchmarkingSuite.Common.Models
{
	[Table (Name = "runs"), JsonObject(MemberSerialization.OptOut)]
	public class Run : DatabaseModel
	{

		[Column (Name = "revision_id"), JsonIgnoreAttribute] //, ForeignKey (ThisKey = "revision_id", ForeignType = typeof (Revision), ForeignKey = "id")]
		public long RevisionID { get; set; }

		[Column (Name = "recipe_id"), JsonIgnoreAttribute] //, ForeignKey (ThisKey = "recipe_id", ForeignType = typeof (Recipe), ForeignKey = "id")]
		public long RecipeID { get; set; }

		[Column (Name = "start_date")]
		public DateTime StartDate { get; set; }

		[Column (Name = "end_date")]
		public DateTime EndDate { get; set; }

		Revision revision = null;
		public Revision Revision {
			get { return revision == null ? (revision = IsConnectionOpen ? Revision.FindByID (RevisionID) : null) : revision; }
			set {
				RevisionID = value.ID;
				revision = value;
			}
		}

		Recipe recipe = null;
		public Recipe Recipe {
			get { return recipe == null ? (recipe = IsConnectionOpen ? Recipe.FindByID (RecipeID) : null) : recipe; }
			set {
				RecipeID = value.ID;
				recipe = value;
			}
		}

		static Run ()
		{
			if (!IsConnectionOpen)
				return;

			new SqliteCommand (@"
				CREATE TABLE IF NOT EXISTS runs (
					id INTEGER PRIMARY KEY AUTOINCREMENT
				      , revision_id INTEGER REFERENCES revisions (id) ON DELETE CASCADE ON UPDATE CASCADE
				      , recipe_id INTEGER REFERENCES recipes (id) ON DELETE CASCADE ON UPDATE CASCADE
				      , start_date DATETIME NOT NULL
				      , end_date DATETIME NOT NULL
				);
			", Connection).ExecuteNonQuery ();
		}

		public static List<Run> All ()
		{
			return All<Run> ();
		}

		public static Run FindByID (long id)
		{
			return FindByID<Run> (id);
		}

		public Run Save ()
		{
			return Save<Run> ();
		}

		public Dictionary<Counter, Dictionary<ulong, object>> GetCounters ()
		{
			throw new NotImplementedException ();
		}
	}
}

