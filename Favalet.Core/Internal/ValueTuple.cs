
#if NET35 || NET40
using System.Diagnostics;
using System.Collections.Generic;

namespace System
{
    [DebuggerStepThrough]
    internal struct ValueTuple<T1, T2>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;

        public ValueTuple(T1 item1, T2 item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }
    }

    [DebuggerStepThrough]
    internal struct ValueTuple<T1, T2, T3>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;

        public ValueTuple(T1 item1, T2 item2, T3 item3)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
        }
    }

    [DebuggerStepThrough]
    internal readonly struct ValueTuple<T1, T2, T3, T4>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;

        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
            this.Item4 = item4;
        }
    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class TupleElementNamesAttribute : Attribute
    {
        public IList<string>? TransformNames { get; }

        public TupleElementNamesAttribute(string[]? transformNames) =>
            this.TransformNames = transformNames;
    }
}
#endif
