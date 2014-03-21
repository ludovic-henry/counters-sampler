using System;

namespace MonoCounters
{
    public enum Category {
        MONO_COUNTER_CAT_JIT,
        MONO_COUNTER_CAT_GC,
        MONO_COUNTER_CAT_METADATA,
        MONO_COUNTER_CAT_GENERICS,
        MONO_COUNTER_CAT_SECURITY,

        MONO_COUNTER_CAT_THREAD,
        MONO_COUNTER_CAT_THREADPOOL,
        MONO_COUNTER_CAT_SYS,

        MONO_COUNTER_CAT_CUSTOM,
    };

    public enum Type {
        MONO_COUNTER_TYPE_INT, /* 4 bytes */
        MONO_COUNTER_TYPE_LONG, /* 8 bytes */
        MONO_COUNTER_TYPE_WORD, /* machine word */
        MONO_COUNTER_TYPE_DOUBLE,
    };

    public enum Unit {
        MONO_COUNTER_UNIT_NONE,  /* It's a raw value that needs special handling from the consumer */
        MONO_COUNTER_UNIT_BYTES, /* Quantity of bytes the counter represent */
        MONO_COUNTER_UNIT_TIME,  /* This is a timestap in 100n units */
        MONO_COUNTER_UNIT_EVENTS, /* Number of times the given event happens */
        MONO_COUNTER_UNIT_CONFIG, /* Configuration knob of the runtime */
        MONO_COUNTER_UNIT_PERCENTAGE, /* Percentage of something */
    };

    public enum Variance {
        MONO_COUNTER_UNIT_CONSTANT = 1, // This counter doesn't change. Agent will only send it once
        MONO_COUNTER_UNIT_MONOTONIC, // This counter value always increase/decreate over time
        MONO_COUNTER_UNIT_VARIABLE, // This counter value can be anything on each sampling
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

        public Counter()
        {
        }

        public Counter(Counter other)
        {
            Category = other.Category;
            Name = other.Name;
            Type = other.Type;
            Unit = other.Unit;
            Variance = other.Variance;
            Value = other.Value;
            Index = other.Index;
        }

        public string CategoryName
        {
            get {
                switch (Category)
                {
                    case Category.MONO_COUNTER_CAT_JIT:
                        return "Mono JIT";
                    case Category.MONO_COUNTER_CAT_GC:
                        return "Mono GC";
                    case Category.MONO_COUNTER_CAT_METADATA:
                        return "Mono Metadata";
                    case Category.MONO_COUNTER_CAT_GENERICS:
                        return "Mono Generics";
                    case Category.MONO_COUNTER_CAT_SECURITY:
                        return "Mono Security";
                    case Category.MONO_COUNTER_CAT_THREAD:
                        return "Mono Thread";
                    case Category.MONO_COUNTER_CAT_THREADPOOL:
                        return "Mono ThreadPool";
                    case Category.MONO_COUNTER_CAT_SYS:
                        return "Mono System";
                    case Category.MONO_COUNTER_CAT_CUSTOM:
                        return "Mono Custom";
                }

                throw new InvalidOperationException();
            }
        }

        public string TypeName
        {
            get {
                switch (Type)
                {
                    case Type.MONO_COUNTER_TYPE_INT:
                        return "int";
                    case Type.MONO_COUNTER_TYPE_LONG:
                        return "long";
                    case Type.MONO_COUNTER_TYPE_WORD:
                        return "word";
                    case Type.MONO_COUNTER_TYPE_DOUBLE:
                        return "double";
                }

                throw new InvalidOperationException();
            }
        }

        public string UnitName
        {
            get {
                switch (Unit)
                {
                    case Unit.MONO_COUNTER_UNIT_NONE:
                        return "none";
                    case Unit.MONO_COUNTER_UNIT_BYTES:
                        return "bytes";
                    case Unit.MONO_COUNTER_UNIT_TIME:
                        return "time";
                    case Unit.MONO_COUNTER_UNIT_EVENTS:
                        return "events";
                    case Unit.MONO_COUNTER_UNIT_CONFIG:
                        return "config";
                    case Unit.MONO_COUNTER_UNIT_PERCENTAGE:
                        return "percentage";
                }

                throw new InvalidOperationException();
            }
        }

        public string VarianceName
        {
            get {
                switch (Variance)
                {
                    case Variance.MONO_COUNTER_UNIT_CONSTANT:
                        return "constant";
                    case Variance.MONO_COUNTER_UNIT_MONOTONIC:
                        return "monotonic";
                    case Variance.MONO_COUNTER_UNIT_VARIABLE:
                        return "variable";
                }

                throw new InvalidOperationException();
            }
        }

        public override String ToString()
        {
            return String.Format("MonoCounter[category={0},name={1},type={2},unit={3},variance={4},value={5},index={6}]", CategoryName, Name, TypeName, UnitName, VarianceName, Value, Index);
        }
    }
}

