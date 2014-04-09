using System;

namespace MonoCounters.Models
{
	public class ForeignKeyAttribute : Attribute
	{
		public System.Type ForeignType { get; set; }
		public string ForeignKey { get; set; }
		public string ThisKey { get; set; }
	}
}

