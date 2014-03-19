using System;

namespace MonoCounters
{
    public enum Category {
        MONO_COUNTER_CAT_JIT,
        MONO_COUNTER_CAT_GC,
        MONO_COUNTER_CAT_METADATA,
        MONO_COUNTER_CAT_GENERICS,
        MONO_COUNTER_CAT_SECURITY,

        MONO_COUNTER_CAT_REMOTING,
        MONO_COUNTER_CAT_EXC,
        MONO_COUNTER_CAT_THREAD,
        MONO_COUNTER_CAT_THREADPOOL,
        MONO_COUNTER_CAT_IO,
    };

    public enum Type {
        MONO_COUNTER_TYPE_INT, /* 4 bytes */
        MONO_COUNTER_TYPE_LONG, /* 8 bytes */
        MONO_COUNTER_TYPE_WORD, /* machine word */
        MONO_COUNTER_TYPE_DOUBLE,

        MONO_COUNTER_TYPE_MAX
    };

    public enum Unit {
        MONO_COUNTER_UNIT_NONE,  // It's a raw value that needs special handling from the consumer
        MONO_COUNTER_UNIT_QUANTITY, // Quantity of the given counter
        MONO_COUNTER_UNIT_TIME,  // This is a timestap in 100n units
        MONO_COUNTER_UNIT_EVENT, // Number of times the given event happens
        MONO_COUNTER_UNIT_CONFIG, // Configuration knob of the runtime
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

        public override String ToString()
        {
            return String.Format("MonoCounter[category={0},name={1},type={2},unit={3},variance={4},value={5},index={6}]", Category, Name, Type, Unit, Variance, Value, Index);
        }
    }
}

