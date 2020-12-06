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
using System.Diagnostics;

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

        private bool AddUnification(
            UnifyContext context,
            IPlaceholderTerm placeholder1,
            IPlaceholderTerm placeholder2,
            UnifyDirections direction,
            bool raiseCouldNotUnify)
        {
            var r1 = context.TryResolve(placeholder1, out var resolved1);
            var r2 = context.TryResolve(placeholder2, out var resolved2);

            switch (r1, r2)
            {
                case (true, true):
                    Debug.WriteLine($"AddUnification: {placeholder1.GetPrettyString(PrettyStringTypes.Minimum)} [{resolved1.GetPrettyString(PrettyStringTypes.Minimum)}] --> {placeholder2.GetPrettyString(PrettyStringTypes.Minimum)} [{resolved2.GetPrettyString(PrettyStringTypes.Minimum)}]");
                    if (resolved1.Equals(resolved2))
                    {
                        return true;
                    }
                    else
                    {
                        return this.InternalUnify(
                            context, resolved1, resolved2, direction, raiseCouldNotUnify);
                    }
                case (true, false):
                    Debug.WriteLine($"AddUnification: {placeholder1.GetPrettyString(PrettyStringTypes.Minimum)} [{resolved1.GetPrettyString(PrettyStringTypes.Minimum)}] --> {placeholder2.GetPrettyString(PrettyStringTypes.Minimum)}");
                    return this.InternalUnify(
                        context, resolved1, placeholder2, direction, raiseCouldNotUnify);
                case (false, true):
                    Debug.WriteLine($"AddUnification: {placeholder1.GetPrettyString(PrettyStringTypes.Minimum)} --> {placeholder2.GetPrettyString(PrettyStringTypes.Minimum)} [{resolved2.GetPrettyString(PrettyStringTypes.Minimum)}]");
                    return this.InternalUnify(
                        context, placeholder1, resolved2, direction, raiseCouldNotUnify);
                default:
                    Debug.WriteLine($"AddUnification: {placeholder1.GetPrettyString(PrettyStringTypes.Minimum)} --> {placeholder2.GetPrettyString(PrettyStringTypes.Minimum)}");
                    context.Add(placeholder1, placeholder2);
                    return true;
            }
        }

        private bool AddUnification(
            UnifyContext context,
            IPlaceholderTerm placeholder,
            IExpression expression,
            UnifyDirections direction,
            bool raiseCouldNotUnify)
        {
            if (context.TryResolve(placeholder, out var resolved))
            {
                Debug.WriteLine($"AddUnification: {placeholder.GetPrettyString(PrettyStringTypes.Minimum)} [{resolved.GetPrettyString(PrettyStringTypes.Minimum)}] --> {expression.GetPrettyString(PrettyStringTypes.Minimum)}");
                return this.InternalUnify(
                    context, resolved, expression, direction, raiseCouldNotUnify);
            }
            else
            {
                Debug.WriteLine($"AddUnification: {placeholder.GetPrettyString(PrettyStringTypes.Minimum)} --> {expression.GetPrettyString(PrettyStringTypes.Minimum)}");
                context.Add(placeholder, expression);
                return true;
            }
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

            switch (expression1, expression2, direction)
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
                case (IFunctionExpression({ } fp, { } fr),
                      IAppliedFunctionExpression({ } tp, { } tr),
                      _):
                    using (context.BeginScope())
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
                        return context.Commit(rp && rr);
                    }

                // Function unification.
                case (IFunctionExpression({ } fp, { } fr),
                      IFunctionExpression({ } tp, { } tr),
                      _):
                    using (context.BeginScope())
                    {
                        // unify(C +> A): Parameters are binder.
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
                        return context.Commit(rp && rr);
                    }
                
                // TODO: IApplyExpression (applicable type functions)
                
                // Placeholder unification.
                case (IPlaceholderTerm fph, IPlaceholderTerm tph, _):
                    return this.AddUnification(
                        context, fph, tph, direction, raiseCouldNotUnify);
                case (IPlaceholderTerm fph, _, UnifyDirections.BiDirectional):
                    return this.AddUnification(
                        context, fph, expression2, UnifyDirections.BiDirectional, raiseCouldNotUnify);
                case (_, IPlaceholderTerm tph, UnifyDirections.BiDirectional):
                    return this.AddUnification(
                        context, tph, expression1, UnifyDirections.BiDirectional, raiseCouldNotUnify);
                case (IPlaceholderTerm fph, _, UnifyDirections.Forward):
                    return this.AddUnification(
                        context, fph, expression2, UnifyDirections.Forward, raiseCouldNotUnify);
                case (IPlaceholderTerm fph, _, UnifyDirections.Backward):
                    return this.AddUnification(
                        context, fph, expression2, UnifyDirections.Backward, raiseCouldNotUnify);
                case (_, IPlaceholderTerm tph, UnifyDirections.Forward):
                    return this.AddUnification(
                        context, tph, expression1, UnifyDirections.Backward, raiseCouldNotUnify);
                case (_, IPlaceholderTerm tph, UnifyDirections.Backward):
                    return this.AddUnification(
                        context, tph, expression1, UnifyDirections.Forward, raiseCouldNotUnify);
                
                // Validate polarity.
                case (_, _, UnifyDirections.Forward):
                    // from <: to
                    var fpf = context.TypeCalculator.Calculate(
                        OrExpression.Create(expression1, expression2, TextRange.Unknown));
                    if (!context.TypeCalculator.Equals(fpf, expression2))
                    {
                        if (raiseCouldNotUnify)
                        {
                            throw new ArgumentException(
                                $"Couldn't unify: {expression1.GetPrettyString(PrettyStringTypes.Minimum)} <: {expression2.GetPrettyString(PrettyStringTypes.Minimum)}");
                        }
                        else
                        {
                            return false;
                        }
                    }
                    break;
                case (_, _, UnifyDirections.Backward):
                    // from :> to
                    var fpr = context.TypeCalculator.Calculate(
                        OrExpression.Create(expression1, expression2, TextRange.Unknown));
                    if (!context.TypeCalculator.Equals(fpr, expression1))
                    {
                        if (raiseCouldNotUnify)
                        {
                            throw new ArgumentException(
                                $"Couldn't unify: {expression1.GetPrettyString(PrettyStringTypes.Minimum)} :> {expression2.GetPrettyString(PrettyStringTypes.Minimum)}");
                        }
                        else
                        {
                            return false;
                        }
                    }
                    break;
                case (_, _, UnifyDirections.BiDirectional):
                    // from == to
                    if (!context.TypeCalculator.Equals(expression1, expression2))
                    {
                        if (raiseCouldNotUnify)
                        {
                            throw new ArgumentException(
                                $"Couldn't unify: {expression1.GetPrettyString(PrettyStringTypes.Minimum)} == {expression2.GetPrettyString(PrettyStringTypes.Minimum)}");
                        }
                        else
                        {
                            return false;
                        }
                    }
                    break;
            }

            return true;
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

            switch (expression1, expression2)
            {
                // Ignore IIgnoreUnificationTerm unification.
                case (IIgnoreUnificationTerm _, _):
                case (_, IIgnoreUnificationTerm _):
                    return true;
            }

            using (context.BeginScope())
            {
                // Unify higher order.
                if (!this.InternalUnify(
                    context,
                    expression1.HigherOrder,
                    expression2.HigherOrder,
                    direction,
                    raiseCouldNotUnify))
                {
                    return context.Commit(false);
                }

                // Unify if succeeded higher order.
                return context.Commit(
                    this.InternalUnifyCore(
                        context,
                        expression1,
                        expression2,
                        direction,
                        raiseCouldNotUnify));
            }
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
