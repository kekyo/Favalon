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

using Favalet.Expressions;
using Favalet.Expressions.Algebraic;
using Favalet.Expressions.Specialized;
using System.Collections.Generic;
using System.Linq;

namespace Favalet.Contexts.Unifiers
{
    partial class UnifyContext
    {
        // Choicer drives with the topology knowledge.
        // This is inserting relations for between placeholders.
        private sealed class TypeTopologyChoicer :
            IExpressionChoicer
        {
            private readonly UnifyContext parent;
            private readonly Dictionary<(IExpression, IExpression), bool> cache =
                new Dictionary<(IExpression, IExpression), bool>();

            private TypeTopologyChoicer(UnifyContext parent) =>
                this.parent = parent;
            
            private IEnumerable<IExpression> TraversePlaceholderTerms<TBinaryExpression>(
                IExpression from,
                UnificationPolarities polarity)
                where TBinaryExpression : IBinaryExpression
            {
                var current = from;
                while (true)
                {
                    if (current is IPlaceholderTerm ph)
                    {
                        var aph = this.parent.GetAlias(ph, ph)!;
                        yield return aph;

                        if (this.parent.topology.TryGetValue(aph, out var node))
                        {
                            foreach (var unification in node.Unifications.Where(
                                unification => unification.Polarity == polarity))
                            {
                                foreach (var expression in
                                    this.TraversePlaceholderTerms<TBinaryExpression>(
                                    unification.Expression, polarity))
                                {
                                    yield return expression;
                                }
                            }
                        }
                    }
                    else if (current is TBinaryExpression binary)
                    {
                        foreach (var expression in
                            this.TraversePlaceholderTerms<TBinaryExpression>(binary.Left, polarity))
                        {
                            yield return expression;
                        }
                        foreach (var expression in
                            this.TraversePlaceholderTerms<TBinaryExpression>(binary.Right, polarity))
                        {
                            yield return expression;
                        }
                    }
                    else
                    {
                        yield return current;
                    }
                    break;
                }
            }

            private bool IsAssignableFrom<TBinaryExpression>(
                IExpression to, IExpression from)
                where TBinaryExpression : IBinaryExpression
            {
                if (this.cache.TryGetValue((from, to), out var result))
                {
                    return result;
                }
                
                if (TraversePlaceholderTerms<TBinaryExpression>(from, UnificationPolarities.Out).
                    Any(expression => this.parent.TypeCalculator.Equals(expression, to)))
                {
                    this.cache.Add((from, to), true);
                    return true;
                }
                else if (TraversePlaceholderTerms<TBinaryExpression>(to, UnificationPolarities.In).
                    Any(expression => this.parent.TypeCalculator.Equals(expression, from)))
                {
                    this.cache.Add((from, to), true);
                    return true;
                }
                else
                {
                    this.cache.Add((from, to), false);
                    return false;
                }
            }

            public ChoiceResults ChoiceForAnd(
                ILogicalCalculator calculator,
                IExpressionChoicer self,
                IExpression left, IExpression right)
            {
                // Narrowing
                var rtl = IsAssignableFrom<AndExpression>(left, right);
                var ltr = IsAssignableFrom<AndExpression>(right, left);

                switch (rtl, ltr)
                {
                    case (true, true):
                        return ChoiceResults.Equal;
                    case (true, false):
                        return ChoiceResults.AcceptRight;
                    case (false, true):
                        return ChoiceResults.AcceptLeft;
                }

                return this.parent.TypeCalculator.DefaultChoicer.ChoiceForAnd(
                    calculator, self, left, right);
            }

            public ChoiceResults ChoiceForOr(
                ILogicalCalculator calculator,
                IExpressionChoicer self,
                IExpression left, IExpression right)
            {
                // Widening
                var rtl = IsAssignableFrom<OrExpression>(left, right);
                var ltr = IsAssignableFrom<OrExpression>(right, left);

                switch (rtl, ltr)
                {
                    case (true, true):
                        return ChoiceResults.Equal;
                    case (true, false):
                        return ChoiceResults.AcceptLeft;
                    case (false, true):
                        return ChoiceResults.AcceptRight;
                }
                
                return this.parent.TypeCalculator.DefaultChoicer.ChoiceForOr(
                    calculator, self, left, right);
            }
        
            public static TypeTopologyChoicer Create(UnifyContext parent) =>
                new TypeTopologyChoicer(parent);
        }
    }
}
