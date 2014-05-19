using System;

namespace MonoCounters.Common
{
	public enum Category
	{
		JIT = 1 << 8,
		GC = 1 << 9,
		Metadata = 1 << 10,
		Generics = 1 << 11,
		Security = 1 << 12,
		Runtime = 1 << 13,
		System = 1 << 14,
	};

	public enum Type
	{
		Int = 0,
		UInt = 1,
		Word = 2,
		Long = 3,
		ULong = 4,
		Double = 5,
		String = 6,
		TimeInterval = 7,
	};

	public enum Unit
	{
		Raw = 0 << 24,
		Bytes = 1 << 24,
		Time = 2 << 24,
		Count = 3 << 24,
		Percentage = 4 << 24,
	};

	public enum Variance
	{
		Monotonic = 1 << 28,
		Constant  = 1 << 29,
		Variable  = 1 << 30,
	};

	public class Counter
	{
		public Category Category { get; set; }
		public String   Name     { get; set; }
		public Type     Type     { get; set; }
		public Unit     Unit     { get; set; }
		public Variance Variance { get; set; }
		public Object   Value    { get; set; }
		public ulong    Index    { get; set; }

		public Counter ()
		{
		}

		public Counter (Counter other)
		{
			Category = other.Category;
			Name = other.Name;
			Type = other.Type;
			Unit = other.Unit;
			Variance = other.Variance;
			Value = other.Value;
			Index = other.Index;
		}

		public string CategoryName {
			get {
				switch (Category) {
				case Category.JIT:
					return "Mono JIT";
				case Category.GC:
					return "Mono GC";
				case Category.Metadata:
					return "Mono Metadata";
				case Category.Generics:
					return "Mono Generics";
				case Category.Security:
					return "Mono Security";
				case Category.System:
					return "Mono System";
				}

				throw new InvalidOperationException ();
			}
		}

		public string TypeName {
			get {
				switch (Type) {
				case Type.Int:
					return "int";
				case Type.UInt:
					return "uint";
				case Type.Long:
					return "long";
				case Type.ULong:
					return "ulong";
				case Type.Word:
					return "word";
				case Type.Double:
					return "double";
				case Type.String:
					return "string";
				case Type.TimeInterval:
					return "time interval";
				}

				throw new InvalidOperationException ();
			}
		}

		public string UnitName {
			get {
				switch (Unit) {
				case Unit.Raw:
					return "raw";
				case Unit.Bytes:
					return "bytes";
				case Unit.Time:
					return "time";
				case Unit.Count:
					return "count";
				case Unit.Percentage:
					return "percentage";
				}

				throw new InvalidOperationException ();
			}
		}

		public string VarianceName {
			get {
				switch (Variance) {
				case Variance.Constant:
					return "constant";
				case Variance.Monotonic:
					return "monotonic";
				case Variance.Variable:
					return "variable";
				}

				throw new InvalidOperationException ();
			}
		}

		public override String ToString ()
		{
			return String.Format ("MonoCounter[category={0},name={1},type={2},unit={3},variance={4},value={5},index={6}]",
				CategoryName, Name, TypeName, UnitName, VarianceName, Value, Index);
		}
	}
}

