/////////////////////////////////////////////////////////////////////////////////////////////////
//
// Favalon - An Interactive Shell Based on a Typed Lambda Calculus.
// Copyright (c) 2018-2020 Kouji Matsui (@kozy_kekyo, @kekyo2)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
/////////////////////////////////////////////////////////////////////////////////////////////////

#if NET35 || NET40

using System.Diagnostics;
using System.Collections.Generic;

namespace System
{
    // Formally the unit type in Favalon.
    [DebuggerStepThrough]
    public readonly struct ValueTuple
    {
        public bool Equals(ValueTuple rhs) =>
            true;
        public override bool Equals(object? obj) =>
            obj is ValueTuple;
        public override int GetHashCode() =>
            0;
        public override string ToString() =>
            "()";
    }

    [DebuggerStepThrough]
    public readonly struct ValueTuple<T1>
    {
        public readonly T1 Item1;

        public ValueTuple(T1 item1) =>
            this.Item1 = item1;

        public bool Equals(ValueTuple<T1> rhs) =>
            this.Item1?.Equals(rhs.Item1) ?? false;
        public override bool Equals(object? obj) =>
            obj is ValueTuple<T1> rhs && this.Equals(rhs);
        public override int GetHashCode() =>
            this.Item1?.GetHashCode() ?? 0;
        public override string ToString() =>
            $"({this.Item1})";
    }

    [DebuggerStepThrough]
    public readonly struct ValueTuple<T1, T2>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;

        public ValueTuple(T1 item1, T2 item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }

        public bool Equals(ValueTuple<T1, T2> rhs) =>
            (this.Item1?.Equals(rhs.Item1) ?? false) &&
            (this.Item2?.Equals(rhs.Item2) ?? false);
        public override bool Equals(object? obj) =>
            obj is ValueTuple<T1, T2> rhs && this.Equals(rhs);
        public override int GetHashCode() =>
            (this.Item1?.GetHashCode() ?? 0) ^
            (this.Item2?.GetHashCode() ?? 0);
        public override string ToString() =>
            $"({this.Item1}, {this.Item2})";
    }

    [DebuggerStepThrough]
    public readonly struct ValueTuple<T1, T2, T3>
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

        public bool Equals(ValueTuple<T1, T2, T3> rhs) =>
            (this.Item1?.Equals(rhs.Item1) ?? false) &&
            (this.Item2?.Equals(rhs.Item2) ?? false) &&
            (this.Item3?.Equals(rhs.Item3) ?? false);
        public override bool Equals(object? obj) =>
            obj is ValueTuple<T1, T2, T3> rhs && this.Equals(rhs);
        public override int GetHashCode() =>
            (this.Item1?.GetHashCode() ?? 0) ^
            (this.Item2?.GetHashCode() ?? 0) ^
            (this.Item3?.GetHashCode() ?? 0);
        public override string ToString() =>
            $"({this.Item1}, {this.Item2}, {this.Item3})";
    }

    [DebuggerStepThrough]
    public readonly struct ValueTuple<T1, T2, T3, T4>
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

        public bool Equals(ValueTuple<T1, T2, T3, T4> rhs) =>
            (this.Item1?.Equals(rhs.Item1) ?? false) &&
            (this.Item2?.Equals(rhs.Item2) ?? false) &&
            (this.Item3?.Equals(rhs.Item3) ?? false) &&
            (this.Item4?.Equals(rhs.Item4) ?? false);
        public override bool Equals(object? obj) =>
            obj is ValueTuple<T1, T2, T3, T4> rhs && this.Equals(rhs);
        public override int GetHashCode() =>
            (this.Item1?.GetHashCode() ?? 0) ^
            (this.Item2?.GetHashCode() ?? 0) ^
            (this.Item3?.GetHashCode() ?? 0) ^
            (this.Item4?.GetHashCode() ?? 0);
        public override string ToString() =>
            $"({this.Item1}, {this.Item2}, {this.Item3}, {this.Item4})";
    }

    [DebuggerStepThrough]
    public readonly struct ValueTuple<T1, T2, T3, T4, T5>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;
        public readonly T4 Item4;
        public readonly T5 Item5;

        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
            this.Item4 = item4;
            this.Item5 = item5;
        }

        public bool Equals(ValueTuple<T1, T2, T3, T4, T5> rhs) =>
            (this.Item1?.Equals(rhs.Item1) ?? false) &&
            (this.Item2?.Equals(rhs.Item2) ?? false) &&
            (this.Item3?.Equals(rhs.Item3) ?? false) &&
            (this.Item4?.Equals(rhs.Item3) ?? false) &&
            (this.Item5?.Equals(rhs.Item4) ?? false);
        public override bool Equals(object? obj) =>
            obj is ValueTuple<T1, T2, T3, T4, T5> rhs && this.Equals(rhs);
        public override int GetHashCode() =>
            (this.Item1?.GetHashCode() ?? 0) ^
            (this.Item2?.GetHashCode() ?? 0) ^
            (this.Item3?.GetHashCode() ?? 0) ^
            (this.Item4?.GetHashCode() ?? 0) ^
            (this.Item5?.GetHashCode() ?? 0);
        public override string ToString() =>
            $"({this.Item1}, {this.Item2}, {this.Item3}, {this.Item4}, {this.Item5})";
    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.All)]
    public sealed class TupleElementNamesAttribute : Attribute
    {
        public IList<string>? TransformNames { get; }

        public TupleElementNamesAttribute(string[]? transformNames) =>
            this.TransformNames = transformNames;
    }
}

#else

using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(System.ValueTuple))]
[assembly: TypeForwardedTo(typeof(System.ValueTuple<>))]
[assembly: TypeForwardedTo(typeof(System.ValueTuple<,>))]
[assembly: TypeForwardedTo(typeof(System.ValueTuple<,,>))]
[assembly: TypeForwardedTo(typeof(System.ValueTuple<,,,>))]
[assembly: TypeForwardedTo(typeof(System.ValueTuple<,,,,>))]
[assembly: TypeForwardedTo(typeof(System.Runtime.CompilerServices.TupleElementNamesAttribute))]

#endif
