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

using Favalet.Expressions.Specialized;
using Favalet.Internal;
using Favalet.Ranges;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Favalet.Expressions.Algebraic
{
    public enum ChoiceResults
    {
        NonRelated,
        Equal,
        AcceptLeft,
        AcceptRight,
    }

    public interface IChoicer
    {
        ChoiceResults ChoiceForAnd(
            ILogicalCalculator calculator,
            IExpression left, IExpression right);

        ChoiceResults ChoiceForOr(
            ILogicalCalculator calculator,
            IExpression left, IExpression right);
    }

    public interface ILogicalCalculator
    {
        bool Equals(IExpression lhs, IExpression rhs);
        bool ExactEquals(IExpression lhs, IExpression rhs);

        IExpression Compute(IExpression operand);
        IExpression Compute(IExpression operand, IChoicer choicer);
    }

    public class LogicalCalculator :
        ILogicalCalculator
    {
        [DebuggerStepThrough]
        public class LogicalCalculatorChoicer :
            IChoicer
        {
            protected LogicalCalculatorChoicer()
            { }

            public virtual ChoiceResults ChoiceForAnd(
                ILogicalCalculator calculator,
                IExpression left, IExpression right) =>
                // Idempotence
                calculator.Equals(left, right) ?
                    ChoiceResults.Equal :
                    ChoiceResults.NonRelated;

            public virtual ChoiceResults ChoiceForOr(
                ILogicalCalculator calculator,
                IExpression left, IExpression right) =>
                // Idempotence
                calculator.Equals(left, right) ?
                    ChoiceResults.Equal :
                    ChoiceResults.NonRelated;

            public static readonly LogicalCalculatorChoicer Instance =
                new LogicalCalculatorChoicer();
        }
    
        [DebuggerStepThrough]
        protected LogicalCalculator()
        {
        }
        
        public virtual IChoicer DefaultChoicer =>
            LogicalCalculatorChoicer.Instance;

        protected virtual IComparer<IExpression>? Sorter =>
            null;

        protected IEnumerable<IExpression> SortExpressions(
            Func<IExpression, IExpression> selector,
            IEnumerable<IExpression> enumerable) =>
            (this.Sorter != null) ?
                enumerable.OrderBy(selector, this.Sorter) :
                enumerable;

        private IEnumerable<IExpression> SortExpressions(
            IEnumerable<IExpression> enumerable) =>
            this.SortExpressions(expression => expression, enumerable);

        public bool Equals(IExpression lhs, IExpression rhs)
        {
            if (object.ReferenceEquals(lhs, rhs))
            {
                return true;
            }
            else
            {
                var left = FlattenedExpression.FlattenAll(lhs, this.SortExpressions);
                var right = FlattenedExpression.FlattenAll(rhs, this.SortExpressions);

                return left.Equals(right);
            }
        }

        public bool ExactEquals(IExpression lhs, IExpression rhs)
        {
            if (object.ReferenceEquals(lhs, rhs))
            {
                return true;
            }
            else
            {
                var left = FlattenedExpression.FlattenAll(lhs, this.SortExpressions);
                var right = FlattenedExpression.FlattenAll(rhs, this.SortExpressions);

                return
                    left.Equals(right) &&
                    (left, right) switch
                    {
                        (DeadEndTerm _, DeadEndTerm _) => true,
                        (DeadEndTerm _, _) => false,
                        (_, DeadEndTerm _) => false,
                        _ => this.Equals(
                            this.Compute(lhs.HigherOrder),
                            this.Compute(rhs.HigherOrder))
                    };
            }
        }

        private IEnumerable<IExpression> ComputeAbsorption<TFlattenedExpression>(
            IExpression left,
            IExpression right,
            Func<ILogicalCalculator, IExpression, IExpression, ChoiceResults> selector)
            where TFlattenedExpression : FlattenedExpression
        {
            var fl = FlattenedExpression.Flatten(left, this.SortExpressions);
            var fr = FlattenedExpression.Flatten(right, this.SortExpressions);

            if (fr is TFlattenedExpression(IExpression[] rightOperands))
            {
                return rightOperands.
                    SelectMany(rightOperand =>
                        selector(this, fl, rightOperand) switch
                        {
                            ChoiceResults.Equal => new[] { fl },
                            ChoiceResults.AcceptLeft => new[] { fl },
                            ChoiceResults.AcceptRight => new[] { rightOperand },
                            _ => Enumerable.Empty<IExpression>()
                        });
            }
            else if (fl is TFlattenedExpression(IExpression[] leftOperands))
            {
                return leftOperands.
                    SelectMany(leftOperand =>
                        selector(this, leftOperand, fr) switch
                        {
                            ChoiceResults.Equal => new[] { leftOperand },
                            ChoiceResults.AcceptLeft => new[] { leftOperand },
                            ChoiceResults.AcceptRight => new[] { fr },
                            _ => Enumerable.Empty<IExpression>()
                        });
            }
            else
            {
                return Enumerable.Empty<IExpression>();
            }
        }

        private IEnumerable<IExpression> ComputeShrink<TBinaryExpression>(
            IExpression left,
            IExpression right,
            Func<ILogicalCalculator, IExpression, IExpression, ChoiceResults> selector)
            where TBinaryExpression : IBinaryExpression
        {
            var flattened = FlattenedExpression.Flatten<TBinaryExpression>(left, right);
            var candidates = new LinkedList<IExpression>(flattened);

            bool requiredRecompute;
            do
            {
                requiredRecompute = false;

                var origin = candidates.First;
                while (origin != null)
                {
                    var current = origin.Next;
                    while (current != null)
                    {
                        // Idempotence / Commutative / Associative
                        if (origin.Value.Equals(current.Value))
                        {
                            candidates.Remove(current);
                            requiredRecompute = true;
                        }
                        // The pair are both type term.
                        else
                        {
                            switch (selector(this, origin.Value, current.Value))
                            {
                                case ChoiceResults.Equal:
                                case ChoiceResults.AcceptLeft:
                                    current.Value = origin.Value;
                                    requiredRecompute = true;
                                    break;
                                case ChoiceResults.AcceptRight:
                                    origin.Value = current.Value;
                                    requiredRecompute = true;
                                    break;
                            }
                        }

                        current = current.Next;
                    }

                    origin = origin.Next;
                }
            }
            while (requiredRecompute);

            return candidates;
        }

        public IExpression Compute(IExpression operand) =>
            this.Compute(operand, this.DefaultChoicer);

        public IExpression Compute(IExpression operand, IChoicer choicer)
        {
            if (operand is IBinaryExpression binary)
            {
                var left = this.Compute(binary.Left);
                var right = this.Compute(binary.Right);

                if (binary is IAndExpression)
                {
                    // Absorption
                    var absorption =
                        this.ComputeAbsorption<OrFlattenedExpression>(left, right, choicer.ChoiceForAnd).
                        Memoize();
                    if (ConstructNested(absorption, OrExpression.Create, binary.Range) is IExpression result1)
                    {
                        return this.Compute(result1);
                    }

                    // Shrink
                    var shrinked =
                        this.ComputeShrink<IAndExpression>(left, right, choicer.ChoiceForAnd).
                        Memoize();
                    if (ConstructNested(shrinked, AndExpression.Create, binary.Range) is IExpression result2)
                    {
                        return result2;
                    }
                }
                else if (binary is IOrExpression)
                {
                    // Absorption
                    var absorption =
                        this.ComputeAbsorption<AndFlattenedExpression>(left, right, choicer.ChoiceForOr).
                        Memoize();
                    if (ConstructNested(absorption, AndExpression.Create, binary.Range) is IExpression result1)
                    {
                        return this.Compute(result1);
                    }

                    // Shrink
                    var shrinked =
                        this.ComputeShrink<IOrExpression>(left, right, choicer.ChoiceForOr).
                        Memoize();
                    if (ConstructNested(shrinked, OrExpression.Create, binary.Range) is IExpression result2)
                    {
                        return result2;
                    }
                }

                // Not changed
                if (object.ReferenceEquals(binary.Left, left) &&
                    object.ReferenceEquals(binary.Right, right))
                {
                    return binary;
                }
                else
                {
                    // Reconstruct
                    if (binary is IAndExpression)
                    {
                        return AndExpression.Create(left, right, binary.Range);
                    }
                    else if (binary is IOrExpression)
                    {
                        return OrExpression.Create(left, right, binary.Range);
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            return operand;
        }

        public static IExpression? ConstructNested(
            IExpression[] results,
            Func<IExpression, IExpression, TextRange, IExpression> creator,
            TextRange range) =>
            results.Length switch
            {
                0 => null,
                1 => results[0],
                2 => creator(results[0], results[1], range),
                _ => results.Skip(2).
                    Aggregate(
                        creator(results[0], results[1], range),
                        (agg, v) => creator(agg, v, range))  // TODO: range
            };

        public static readonly LogicalCalculator Instance =
            new LogicalCalculator();
    }
}
