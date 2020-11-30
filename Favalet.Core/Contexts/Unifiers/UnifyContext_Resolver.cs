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
using System.Linq;
using Favalet.Expressions;
using Favalet.Expressions.Algebraic;
using Favalet.Expressions.Specialized;
using Favalet.Internal;
using Favalet.Ranges;

namespace Favalet.Contexts.Unifiers
{
    partial class UnifyContext
    {
#if true
        private IExpression InternalResolve(
            IPlaceholderTerm placeholder,
            UnificationPolarities direction,
            Func<IExpression, IExpression, IExpression, TextRange, IExpression> creator)
        {
            if (this.topology.TryGetValue(placeholder, out var node))
            {
                var expressionsByDirection = node.Unifications.
                    Where(unification => 
                        (unification.Polarity == direction) ||
                        (unification.Polarity == UnificationPolarities.Both)).
                    Select(unification => unification.Expression switch
                    {
                        IPlaceholderTerm ph => this.InternalResolve(ph, direction, creator),
                        _ => unification.Expression
                    }).
                    Memoize();
                
                if (expressionsByDirection.Length >= 1)
                {
                    var combined = LogicalCalculator.ConstructNested(
                        expressionsByDirection, UnspecifiedTerm.Instance, creator, TextRange.Unknown)!;
                    return combined;
                }
            }

            return placeholder;
        }
        
        public IExpression? Resolve(IPlaceholderTerm placeholder)
        {
            var normalized = this.GetAlias(placeholder, placeholder)!;
            
            var narrow = this.InternalResolve(
                normalized,
                UnificationPolarities.In,
                AndExpression.Create);
            if (!narrow.IsContainsPlaceholder(true))
            {
                return narrow;
            }
            
            var widen = this.InternalResolve(
                normalized,
                UnificationPolarities.Out,
                OrExpression.Create);
            if (!widen.IsContainsPlaceholder(true))
            {
                return widen;
            }

            if (!narrow.IsContainsPlaceholder(false))
            {
                return narrow;
            }

            if (!widen.IsContainsPlaceholder(false))
            {
                return widen;
            }

            if (!(narrow is IPlaceholderTerm))
            {
                return narrow;
            }
            
            if (!(widen is IPlaceholderTerm))
            {
                return widen;
            }

            return narrow;
        }
#else
        [DebuggerStepThrough]
        private sealed class ResolveContext
        {
            private readonly ITypeCalculator calculator;
            private readonly Func<IExpression, IExpression, TextRange, IExpression> creator;

            public readonly UnificationPolarities Polarity;

            private ResolveContext(
                ITypeCalculator calculator,
                UnificationPolarities polarity, 
                Func<IExpression, IExpression, TextRange, IExpression> creator)
            {
                Debug.Assert(polarity != UnificationPolarities.Both);
                
                this.calculator = calculator;
                this.Polarity = polarity;
                this.creator = creator;
            }

            public IExpression? Compute(IExpression[] expressions, TextRange range) =>
                LogicalCalculator.ConstructNested(expressions, this.creator, range) is IExpression combined ?
                    this.calculator.Calculate(combined) : null;
            
            public static ResolveContext Create(
                ITypeCalculator calculator,
                UnificationPolarities polarity, 
                Func<IExpression, IExpression, TextRange, IExpression> creator) =>
                new ResolveContext(calculator, polarity, creator);
        }
        
        private IExpression InternalResolve(
            ResolveContext context,
            IPlaceholderTerm placeholder)
        {
            var resolved = this.GetAlias(placeholder, placeholder)!;
            if (resolved is IPlaceholderTerm ph)
            {
                if (this.topology.TryGetValue(ph, out var node))
                {
                    IExpression ResolveRecursive(
                        IExpression expression)
                    {
                        switch (expression)
                        {
                            case IPlaceholderTerm ph:
                                return this.InternalResolve(context, ph);
                            case IPairExpression parent:
                                return parent.Create(
                                    ResolveRecursive(parent.Left),
                                    ResolveRecursive(parent.Right),
                                    UnspecifiedTerm.Instance,
                                    parent.Range);
                            default:
                                return expression;
                        }
                    }

                    var expressions = node.Unifications.Where(unification =>
                            (unification.Polarity == context.Polarity) ||
                            (unification.Polarity == UnificationPolarities.Both))
                        .Select(unification => ResolveRecursive(unification.Expression)).ToArray();
                    if (expressions.Length >= 1)
                    {
                        // TODO: placeholder filtering idea is bad?
                        if (expressions.All(expression => expression.IsContainsPlaceholder))
                        {
                            var calculated = context.Compute(
                                expressions, TextRange.Unknown)!; // TODO: range
                            return calculated;
                        }
                        else
                        {
                            var filtered = expressions.Where(expression => !expression.IsContainsPlaceholder).ToArray();
                            var calculated = context.Compute(
                                filtered, TextRange.Unknown)!; // TODO: range
                            return calculated;
                        }
                    }
                }
            }
            return resolved;
        }
        
        public override IExpression? Resolve(IPlaceholderTerm placeholder)
        {
            // TODO: cache
            
            var outMost0 = this.InternalResolve(
                ResolveContext.Create(
                    this.TypeCalculator,
                    UnificationPolarities.Out,
                    OrExpression.Create),
                placeholder);
            var inMost0 = this.InternalResolve(
                ResolveContext.Create(
                    this.TypeCalculator,
                    UnificationPolarities.In,
                    AndExpression.Create),
                placeholder);

            switch (outMost0, inMost0)
            {
                case (IPlaceholderTerm _, IPlaceholderTerm imph0):
                    // inmost (narrow) has higher priority.
                    var inMost1 = this.InternalResolve(
                        ResolveContext.Create(
                            this.TypeCalculator,
                            UnificationPolarities.In,
                            AndExpression.Create),
                        imph0);
                    return inMost1;
                case (IPlaceholderTerm _, _):
                    return inMost0;
                case (_, IPlaceholderTerm _):
                    return outMost0;
                default:
                    return inMost0;
                    // Combine both expressions.
                    //return calculator.Compute(
                    //    AndExpression.Create(outMost0, inMost0, placeholder.Range));  // TODO: range
            }
        }
#endif
    }
}