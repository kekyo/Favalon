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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Favalet.Internal
{
    [DebuggerStepThrough]
    internal static class EnumerableEx
    {
        public static T[] Memoize<T>(
            this IEnumerable<T> enumerable) =>
            enumerable switch
            {
                T[] array => array,
                List<T> list => list.ToArray(),
                _ => enumerable.ToArray()
            };

        // unfold
        public static IEnumerable<U> Traverse<T, U>(
            this T? seed,
            Func<T, U?> predicate)
            where T : class
            where U : class, T
        {
            U? value = seed as U;
            while (value != null)
            {
                yield return value;
                value = predicate(value);
            }
        }

        public static IEnumerable<U> Collect<T, U>(
            this IEnumerable<T> enumerable,
            Func<T, U?> predicate)
            where U : class
        {
            foreach (var value in enumerable)
            {
                if (predicate(value) is U v)
                {
                    yield return v;
                }
            }
        }

#if NET40 || NET45 || NETSTANDARD1_1
        public static IEnumerable<T> Append<T>(
            this IEnumerable<T> enumerable,
            T value)
        {
            foreach (var v in enumerable)
            {
                yield return v;
            }
            yield return value;
        }
#endif

        public static bool EqualsPartiallyOrdered<T>(
            this IEnumerable<T> lhs,
            IEnumerable<T> rhs)
        {
            var l = new HashSet<T>(lhs);
            var r = new HashSet<T>(rhs);
            return lhs.All(r.Contains) && rhs.All(l.Contains);
        }
    }
}
