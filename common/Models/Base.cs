using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.IO;
using System.Reflection;
using System.Text;
using Mono.Data.Sqlite;

namespace MonoCounters.Web.Models
{
	public abstract class Base
	{
		[Column (Name = "id", IsPrimaryKey = true)]
		public long ID { get; protected set; }

		public static SqliteConnection Connection { get; private set; }

		protected bool IsNew { get; set; }

		public Base () : this (true)
		{
		}

		protected Base (bool isNew)
		{
			ID = -1;
			IsNew = isNew;
		}

		static Base ()
		{
			Connection = new SqliteConnection (String.Format ("Data Source={0};Version=3;", 
				Path.Combine (Path.GetDirectoryName (Assembly.GetExecutingAssembly ().FullName), "..", "..", "Models", "database.sqlite")));

			Connection.Open ();
		}

		public static T FindByID<T> (long id) where T : Base
		{
			var table = GetTable (typeof(T));
			var fields = GetFields (typeof(T));

			var sql = new SqliteCommand (String.Format ("SELECT id, {0} FROM {1} where id = ? LIMIT 1", String.Join (", ", fields.Keys), table), Connection);

			sql.Parameters.Add (new SqliteParameter () { Value = id });

			using (var reader = sql.ExecuteReader ()) {
				while (reader.Read ()) {
					var constructor = typeof (T).GetConstructor (BindingFlags.Instance | BindingFlags.NonPublic, null, new [] { typeof (Boolean) }, null);
					var record = (T) constructor.Invoke (new object[] { false });

					for (int i = 1; i < reader.FieldCount; i++) {
						// Name of the SQL field
						var name = reader.GetName (i);

						if (!fields.ContainsKey (name))
							continue;

						// Get PropertyInfo for given SQL field
						var field = fields [name];
						var attributes = field.GetCustomAttributes (typeof (ForeignKeyAttribute), true);

						object value;

						if (attributes.Length == 0) {
							// If this field has no ForeignKey attribute, it means it is a simple field
							// TODO : check that it's working for every types
							value = reader.GetValue (i);
						} else {
							// If this field has a ForeignKey attribute, then we also fetch the value of this field
							var type = ((ForeignKeyAttribute)attributes [0]).ForeignType;
							var method = type.GetMethod ("FindByID", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);

							// In case the method has be reimplemented by the model T
							value = (method.IsGenericMethodDefinition ? method.MakeGenericMethod (new System.Type [] { type }) : method)
									.Invoke (null, new object [] { reader.GetInt64 (i) });
						}

						fields [name].SetValue (record, value, null);
					}

					return record;
				}
			}

			return null;
		}

		public T Save<T> () where T : Base
		{
			var table = GetTable (typeof (T));
			var fields = GetFields (typeof (T));

			// Generate SQL query
			SqliteCommand sql;

			if (IsNew) {
				if (fields.Count == 0) {
					sql = new SqliteCommand (String.Format ("INSERT INTO {0} (id) VALUES (NULL)", table), Connection);
				} else {
					sql = new SqliteCommand (String.Format ("INSERT INTO {0} ({1}) VALUES ({2})", table, String.Join (",", fields.Keys), 
						String.Join (",", System.Linq.Enumerable.Repeat<string> ("?", fields.Keys.Count))), Connection);
				}
			} else {
				if (fields.Count == 0)
					return (T)this;

				var values = new List<string> ();

				foreach (var column in fields.Keys) {
					if (column.Equals ("id"))
						continue;

					values.Add (String.Format ("{0} = ?", column));
				}

				sql = new SqliteCommand (String.Format ("UPDATE {0} SET {1} WHERE id = {2}", table, String.Join (",", values), this.ID), Connection);
			}

			// Add parameters to SQL query
			foreach (var field in fields) {
				if (field.Key.Equals ("id"))
					continue;

				var property = field.Value;
				var attributes = property.GetCustomAttributes (typeof (ForeignKeyAttribute), true);
				var value = property.GetValue (this, null);

				if (value == null)
					sql.Parameters.Add (new SqliteParameter () { Value = null });
				else if (attributes.Length == 0)
					sql.Parameters.Add (new SqliteParameter () { Value = value });
				else
					sql.Parameters.Add (new SqliteParameter () { Value = ((Base)value).ID });
			}

			sql.ExecuteNonQuery ();

			// Get ID in case it's new
			if (IsNew) {
				var result = new SqliteCommand ("SELECT last_insert_rowid()", Connection).ExecuteScalar ();

				if (result == null)
					throw new SqliteException ();

				ID = (long)result;
				IsNew = false;
			}

			return (T)this;
		}

		static string GetTable (System.Type type)
		{
			var attributes = type.GetCustomAttributes (typeof(TableAttribute), true);

			if (attributes.Length == 0)
				throw new Exception ("Missing Table annotation on this class");

			return ((TableAttribute)attributes.GetValue (0)).Name;
		}

		static Dictionary<string, PropertyInfo> GetFields (System.Type type)
		{
			var fields = new Dictionary<string, PropertyInfo> ();

			foreach (var property in type.GetProperties()) {
				var attributes = property.GetCustomAttributes (typeof(ColumnAttribute), true);

				if (attributes.Length > 0) {
					var name = ((ColumnAttribute)attributes.GetValue (0)).Name;

					if (name.Equals ("id"))
						continue;

					fields.Add (name, property);
				}
			}

			return fields;
		}
	}
}
