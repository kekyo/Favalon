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
using System.Linq;
using Favalet.Ranges;

namespace Favalet.Expressions.Specialized
{
    public interface IPairExpression :
        IExpression
    {
        IExpression Left { get; }
        IExpression Right { get; }
        
        Type IdentityType { get; }

        IExpression Create(IExpression left, IExpression right, TextRange range);
    }

    public static class PairExpressionExtension
    {
        public static void Deconstruct(
            this IPairExpression pair, out IExpression left, out IExpression right)
        {
            left = pair.Left;
            right = pair.Right;
        }
        
        public static IEnumerable<IExpression> Children(this IPairExpression pair)
        {
            yield return pair.Left;
            yield return pair.Right;
        }
        
        public static IExpression? Create(
            this IPairExpression pair,
            IEnumerable<IExpression> children,
            TextRange range) =>
            children.ToArray() switch
            {
                { Length: 2 } arr =>
                    pair.Create(arr[0], arr[1], range),
                _ =>
                    throw new InvalidOperationException()
            };
    }
}
