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

#pragma warning disable CS8601

namespace Favalet.Internal
{
    internal static class LazySlim
    {
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        public static LazySlim<T> Create<T>(Func<T> generator) =>
            new LazySlim<T>(generator);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        public static LazySlim<T> Create<T>(T value) =>
            new LazySlim<T>(value);
    }

    [DebuggerStepThrough]
    internal sealed class LazySlim<T>
    {
        private volatile Func<T>? generator;
        private T value;

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        public LazySlim(Func<T> generator)
        {
            this.generator = generator;
            this.value = default!;
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        public LazySlim(T value)
        {
            this.generator = default;
            this.value = value;
        }

        public T Value
        {
            [DebuggerStepThrough]
            get
            {
                var generator = this.generator;
                if (generator != null)
                {
                    this.value = generator();
                    this.generator = null;
                }

                return value;
            }
        }

        public override string? ToString() =>
            (this.generator != null) ? "(Not generated)" : this.value?.ToString();
    }
}
