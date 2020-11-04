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

using System.Diagnostics;
using Favalet.Expressions;
using System.Runtime.CompilerServices;

namespace Favalet.Parsers
{
    public struct ParseRunnerResult
    {
        public readonly ParseRunner Next;
        public readonly IExpression? Expression;

#if NET45 || NETSTANDARD1_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        private ParseRunnerResult(ParseRunner next, IExpression? expression)
        {
            this.Next = next;
            this.Expression = expression;
        }

#if NET45 || NETSTANDARD1_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        public static ParseRunnerResult Empty(ParseRunner next) =>
            new ParseRunnerResult(next, null);

#if NET45 || NETSTANDARD1_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        public static ParseRunnerResult Create(ParseRunner next, IExpression? expression) =>
            new ParseRunnerResult(next, expression);

#if NET45 || NETSTANDARD1_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        public void Deconstruct(out ParseRunner next, out IExpression? expression)
        {
            next = this.Next;
            expression = this.Expression;
        }
    }
}