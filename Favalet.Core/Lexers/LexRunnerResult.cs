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

using Favalet.Tokens;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Favalet.Lexers
{
    internal struct LexRunnerResult
    {
        public readonly LexRunner Next;
        public readonly Token? Token0;
        public readonly Token? Token1;

#if NET45 || NETSTANDARD1_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        private LexRunnerResult(LexRunner next, Token? token0, Token? token1)
        {
            this.Next = next;
            this.Token0 = token0;
            this.Token1 = token1;
        }

#if NET45 || NETSTANDARD1_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        public static LexRunnerResult Empty(LexRunner next) =>
            new LexRunnerResult(next, null, null);

#if NET45 || NETSTANDARD1_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        public static LexRunnerResult Create(LexRunner next, Token? token0) =>
            new LexRunnerResult(next, token0, null);

#if NET45 || NETSTANDARD1_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        public static LexRunnerResult Create(LexRunner next, Token? token0, Token? token1) =>
            new LexRunnerResult(next, token0, token1);

#if NET45 || NETSTANDARD1_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        public void Deconstruct(out LexRunner next, out Token? token0, out Token? token1)
        {
            next = this.Next;
            token0 = this.Token0;
            token1 = this.Token1;
        }
    }
}
