using System;

namespace MonoCounters
{
	public enum Category
	{
		JIT,
		GC,
		Metadata,
		Generics,
		Security,
		Thread,
		ThreadPool,
		System,
		Custom,
	};

	public enum Type
	{
		Int,
		/* 4 bytes */
		Long,
		/* 8 bytes */
		Word,
		/* machine word */
		Double,
	};

	public enum Unit
	{
		None,
		/* It's a raw value that needs special handling from the consumer */
		Bytes,
		/* Quantity of bytes the counter represent */
		Time,
		/* This is a timestap in 100n units */
		Events,
		/* Number of times the given event happens */
		Config,
		/* Configuration knob of the runtime */
		Percentage,
		/* Percentage of something */
	};

	public enum Variance
	{
		Constant = 1,
		// This counter doesn't change. Agent will only send it once
		Monotonic,
		// This counter value always increase/decreate over time
		Variable,
		// This counter value can be anything on each sampling
	};

	public class Counter
	{
		public Category Category { get; set; }

		public String   Name     { get; set; }

		public Type     Type     { get; set; }

		public Unit     Unit     { get; set; }

		public Variance Variance { get; set; }

		public Object   Value    { get; set; }

		public short    Index    { get; set; }

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
				case Category.Thread:
					return "Mono Thread";
				case Category.ThreadPool:
					return "Mono ThreadPool";
				case Category.System:
					return "Mono System";
				case Category.Custom:
					return "Mono Custom";
				}

				throw new InvalidOperationException ();
			}
		}

		public string TypeName {
			get {
				switch (Type) {
				case Type.Int:
					return "int";
				case Type.Long:
					return "long";
				case Type.Word:
					return "word";
				case Type.Double:
					return "double";
				}

				throw new InvalidOperationException ();
			}
		}

		public string UnitName {
			get {
				switch (Unit) {
				case Unit.None:
					return "none";
				case Unit.Bytes:
					return "bytes";
				case Unit.Time:
					return "time";
				case Unit.Events:
					return "events";
				case Unit.Config:
					return "config";
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

