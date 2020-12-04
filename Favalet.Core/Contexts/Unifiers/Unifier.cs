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
using Favalet.Ranges;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Favalet.Internal;

namespace Favalet.Contexts.Unifiers
{
    internal enum UnifyDirections
    {
        Forward,
        Backward,
        BiDirectional
    }
    
    internal sealed partial class Unifier
    {
        [DebuggerStepThrough]
        private Unifier()
        { }

        ///////////////////////////////////////////////////////////////////////////////////

        private bool AddAlias(
            UnifyContext context,
            IExpression from,
            IExpression to,
            bool raiseCouldNotUnify)
        {
            switch (from, to)
            {
                case (IPlaceholderTerm fph, _):
                    if (context.TryAddAlias(fph, to, out var alias1))
                    {
                        return true;
                    }
                    else
                    {
                        return this.InternalUnify(
                            context, alias1, to, UnifyDirections.BiDirectional, raiseCouldNotUnify);
                    }
                case (_, IPlaceholderTerm tph):
                    if (context.TryAddAlias(tph, from, out var alias2))
                    {
                        return true;
                    }
                    else
                    {
                        return this.InternalUnify(
                            context, alias2, from, UnifyDirections.BiDirectional, raiseCouldNotUnify);
                    }
            }

            throw new InvalidOperationException();
        }

        private bool AddUnification(
            UnifyContext context,
            IPlaceholderTerm placeholder,
            IExpression expression,
            UnificationPolarities polarity,
            bool raiseCouldNotUnify)
        {
            if (context.GetOrAddNode(placeholder, expression, polarity, out var node) ==
                GetOrAddNodeResults.Added)
            {
                return true;
            }

            using (context.AllScope())
            {
                var entries = node.Unifications.
                    Where(unification => unification.Polarity == polarity).
                    Select(unification =>
                        (unification,
                         result: this.InternalUnify(
                            context, expression, unification.Expression,
                            polarity switch {
                                UnificationPolarities.Forward => UnifyDirections.Forward,
                                _ => UnifyDirections.Backward
                            },
                            false))).
                    Memoize();

                if (entries.All(entry => !entry.result))
                {
                    if (raiseCouldNotUnify)
                    {
                        throw new InvalidOperationException("Couldn't unify");
                    }
                    else
                    {
                        return false;
                    }
                }
                
                foreach (var entry in entries.Where(entry => !entry.result))
                {
                    node.Unifications.Remove(entry.unification);
                }
            }

            return true;
        }

        ///////////////////////////////////////////////////////////////////////////////////
        
        private bool InternalUnifyCore(
            UnifyContext context,
            IExpression expression1,
            IExpression expression2,
            UnifyDirections direction,
            bool raiseCouldNotUnify)
        {
            Debug.Assert(!(expression1 is IIgnoreUnificationTerm));
            Debug.Assert(!(expression2 is IIgnoreUnificationTerm));

            switch (@from: expression1, to: expression2, direction)
            {
                // Binary expression unification.
                case (IBinaryExpression fb, _, _):
                    var rfl = this.InternalUnify(
                        context, fb.Left, expression2, direction, raiseCouldNotUnify);
                    var rfr = this.InternalUnify(
                        context, fb.Right, expression2, direction, raiseCouldNotUnify);
                    return rfl && rfr;
                case (_, IBinaryExpression tb, _):
                    var rtl = this.InternalUnify(
                        context, expression1, tb.Left, direction, raiseCouldNotUnify);
                    var rtr = this.InternalUnify(
                        context, expression1, tb.Right, direction, raiseCouldNotUnify);
                    return rtl && rtr;

                // Applied function unification.
                case (IFunctionExpression(IExpression fp, IExpression fr),
                      IAppliedFunctionExpression(IExpression tp, IExpression tr),
                      _):
                    using (context.AllScope())
                    {
                        // unify(C +> A): But parameters aren't binder.
                        var rp = this.InternalUnify(
                            context, fp, tp,
                            direction switch
                            {
                                UnifyDirections.Backward => UnifyDirections.Forward,
                                //UnifyDirections.BiDirectional => UnifyDirections.Backward,
                                _ => UnifyDirections.Backward
                            },
                            raiseCouldNotUnify);
                        // unify(B +> D)
                        var rr = this.InternalUnify(
                            context, fr, tr,
                            direction switch
                            {
                                UnifyDirections.Backward => UnifyDirections.Backward,
                                //UnifyDirections.BiDirectional => UnifyDirections.Forward,
                                _ => UnifyDirections.Forward
                            },
                            raiseCouldNotUnify);
                        return rp && rr;
                    }

                // Function unification.
                case (IFunctionExpression(IExpression fp, IExpression fr),
                      IFunctionExpression(IExpression tp, IExpression tr),
                      _):
                    using (context.AllScope())
                    {
                        // unify(C +> A): Parameters are binder.
                        var rp = this.InternalUnify(
                            context, fp, tp,
                            direction switch
                            {
                                UnifyDirections.Backward => UnifyDirections.Forward,
                                _ => UnifyDirections.Backward
                            },
                            raiseCouldNotUnify);
                        // unify(B +> D)
                        var rr = this.InternalUnify(
                            context, fr, tr,
                            direction switch
                            {
                                UnifyDirections.Forward => UnifyDirections.Backward,
                                _ => UnifyDirections.Forward
                            },
                            raiseCouldNotUnify);
                        return rp && rr;
                    }
                
                // TODO: IApplyExpression (applicable type functions)
                
                // Placeholder unification.
                case (IPlaceholderTerm fph, _, UnifyDirections.BiDirectional):
                    return this.AddAlias(context, fph, expression2, raiseCouldNotUnify);
                case (_, IPlaceholderTerm tph, UnifyDirections.BiDirectional):
                    return this.AddAlias(context, tph, expression1, raiseCouldNotUnify);
                case (IPlaceholderTerm fph, _, UnifyDirections.Forward):
                    return this.AddUnification(context, fph, expression2, UnificationPolarities.Forward, raiseCouldNotUnify);
                case (IPlaceholderTerm fph, _, UnifyDirections.Backward):
                    return this.AddUnification(context, fph, expression2, UnificationPolarities.Backward, raiseCouldNotUnify);
                case (_, IPlaceholderTerm tph, UnifyDirections.Forward):
                    return this.AddUnification(context, tph, expression1, UnificationPolarities.Backward, raiseCouldNotUnify);
                case (_, IPlaceholderTerm tph, UnifyDirections.Backward):
                    return this.AddUnification(context, tph, expression1, UnificationPolarities.Forward, raiseCouldNotUnify);
            }
            
            // Validate polarity.
            // from <: to
            var f = context.TypeCalculator.Calculate(
                OrExpression.Create(expression1, expression2, TextRange.Unknown));
            if (!context.TypeCalculator.Equals(f, expression2))
            {
                if (raiseCouldNotUnify)
                {
                    throw new ArgumentException(
                        $"Couldn't unify: {expression1.GetPrettyString(PrettyStringTypes.Minimum)} <: {expression2.GetPrettyString(PrettyStringTypes.Minimum)}");
                }
                else
                {
                    context.MarkCouldNotUnify(expression1, expression2);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////

        private bool InternalUnify(
            UnifyContext context,
            IExpression expression1,
            IExpression expression2,
            UnifyDirections direction,
            bool raiseCouldNotUnify)
        {
            // Same as.
            if (context.TypeCalculator.ExactEquals(expression1, expression2))
            {
                return true;
            }

            switch (@from: expression1, to: expression2)
            {
                // Ignore IIgnoreUnificationTerm unification.
                case (IIgnoreUnificationTerm _, _):
                case (_, IIgnoreUnificationTerm _):
                    return true;
            }

            using (context.AllScope())
            {
                // Unify higher order.
                if (!this.InternalUnify(
                    context,
                    expression1.HigherOrder,
                    expression2.HigherOrder,
                    direction,
                    raiseCouldNotUnify))
                {
                    return false;
                }

                // Unify if succeeded higher order.
                if (!this.InternalUnifyCore(
                    context,
                    expression1,
                    expression2,
                    direction,
                    raiseCouldNotUnify))
                {
                    return false;
                }
            }

            return true;
        }

        ///////////////////////////////////////////////////////////////////////////////////

        [DebuggerStepThrough]
        public void Unify(
            UnifyContext context,
            IExpression expression1,
            IExpression expression2,
            bool bidirectional) =>
            this.InternalUnify(
                context, expression1, expression2,
                bidirectional ? UnifyDirections.BiDirectional : UnifyDirections.Forward,
                true);
        
        public static readonly Unifier Instance =
            new Unifier();
    }
}
