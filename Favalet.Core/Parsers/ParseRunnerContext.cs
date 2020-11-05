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
using Favalet.Expressions;
using Favalet.Tokens;
using Favalet.Contexts;

namespace Favalet.Parsers
{
    [DebuggerStepThrough]
    public class ParseRunnerContext
    {
        private readonly Stack<(ParenthesisPair pair, IExpression? left)> scopes =
            new Stack<(ParenthesisPair pair, IExpression? left)>();
        
        [DebuggerStepThrough]
        protected ParseRunnerContext() =>
            this.LastToken = null;

        public IExpression? Current { get; private set; }
        public Token? LastToken { get; private set; }
        
        [DebuggerStepThrough]
        internal void SetLastToken(Token token) =>
            this.LastToken = token;

        [DebuggerStepThrough]
        private static IExpression Combine(IExpression? left, IExpression? right)
        {
            if (left != null)
            {
                if (right != null)
                {
                    return ApplyExpression.Create(
                        left, right, left.Range.Combine(right.Range));
                }
                else
                {
                    return left;
                }
            }
            else
            {
                return right!;
            }
        }

        [DebuggerStepThrough]
        public void CombineAfter(IExpression expression) =>
            this.Current = Combine(this.Current, expression);
        [DebuggerStepThrough]
        public void CombineBefore(IExpression expression) =>
            this.Current = Combine(expression, this.Current);

        public void PushScope(ParenthesisPair pair)
        {
            this.scopes.Push((pair, this.Current));
            this.Current = null;
        }

        public void PopScope(ParenthesisPair pair)
        {
            if (this.scopes.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Parenthesis mismatched: ? ... {pair.Close}");
            }
            
            var (p, left) = this.scopes.Pop();
            if (!p.Close.Equals(pair.Close))
            {
                throw new InvalidOperationException(
                    $"Parenthesis mismatched: {p.Open} ... {pair.Close}");
            }
            
            this.Current = Combine(left, this.Current);
        }

        [DebuggerStepThrough]
        public override string ToString() =>
            this.Current is IExpression current ?
                $"{current.GetPrettyString(PrettyStringTypes.Readable)}" :
                "(empty)";

        [DebuggerStepThrough]
        public static ParseRunnerContext Create() =>
            new ParseRunnerContext();
    }
}
