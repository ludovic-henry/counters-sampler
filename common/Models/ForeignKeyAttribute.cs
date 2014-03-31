using System;

namespace MonoCounters
{
	public class ForeignKeyAttribute : Attribute
	{
		public System.Type ForeignType { get; set; }
		public string ForeignKey { get; set; }
		public string ThisKey { get; set; }
	}
}

