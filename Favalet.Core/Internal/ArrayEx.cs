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
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Favalet.Internal
{
    [DebuggerStepThrough]
    internal static class ArrayEx
    {
#if NETSTANDARD2_0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Empty<T>() =>
            Array.Empty<T>();
#else
        private static class EmptyHolder<T>
        {
            public static readonly T[] Empty = new T[0];
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T[] Empty<T>() =>
            EmptyHolder<T>.Empty;
#endif
    }
}
