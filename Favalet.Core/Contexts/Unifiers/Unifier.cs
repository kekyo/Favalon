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

        private bool Substitute(
            UnifyContext context,
            IPlaceholderTerm placeholder,
            IExpression expression,
            UnifyDirections direction,
            bool raiseCouldNotUnify)
        {
            if (!context.TryLookup(placeholder, out var unification))
            {
                Debug.WriteLine($"Substitute: {placeholder.GetPrettyString(PrettyStringTypes.Minimum)} --> {expression.GetPrettyString(PrettyStringTypes.Minimum)}");
                context.Set(placeholder, expression, direction);
                return true;
            }

            Debug.WriteLine($"Substitute: {placeholder.GetPrettyString(PrettyStringTypes.Minimum)} [{unification}] --> {expression.GetPrettyString(PrettyStringTypes.Minimum)}");
            switch (unification.Direction, direction)
            {
                // TODO: Rigid bound variable type to free variable type.
                //   CovarianceInLambdaBody1, CovarianceInLambdaBody2
                //   Better : (a -> a):(int -> object) ==> (a:int -> a:int):(int -> object)
                //   Current: (a -> a):(int -> object) ==> (a:int -> a:object):(int -> object)
                // case (UnifyDirections.BiDirectional, UnifyDirections.Forward):
                //     // Check and will not update.
                //     return this.InternalUnify(
                //         context, unification.Expression, expression, direction, raiseCouldNotUnify);
                // case (UnifyDirections.Backward, UnifyDirections.Forward):
                //     // Check and will not update.
                //     return this.InternalUnify(
                //         context, unification.Expression, expression, direction, raiseCouldNotUnify);
                default:
                    context.Set(placeholder, expression, direction);
                    return this.InternalUnify(
                        context, unification.Expression, expression, direction, raiseCouldNotUnify);
            }
        }

        private bool Substitute2(
            UnifyContext context,
            IPlaceholderTerm placeholder1,
            IPlaceholderTerm placeholder2,
            UnifyDirections direction,
            bool raiseCouldNotUnify)
        {
            var r1 = context.TryLookup(placeholder1, out var unification1);
            var r2 = context.TryLookup(placeholder2, out var unification2);

            switch (r1, r2)
            {
                case (true, true):
                    Debug.WriteLine($"Substitute2: {placeholder1.GetPrettyString(PrettyStringTypes.Minimum)} [{unification1}] --> {placeholder2.GetPrettyString(PrettyStringTypes.Minimum)} [{unification2}]");
                    if (unification1.Equals(unification2))
                    {
                        return true;
                    }
                    else
                    {
                        return this.InternalUnify(
                            context, unification1.Expression, unification2.Expression, direction, raiseCouldNotUnify);
                    }
                case (true, false):
                    Debug.WriteLine($"Substitute2: {placeholder1.GetPrettyString(PrettyStringTypes.Minimum)} [{unification1}] --> {placeholder2.GetPrettyString(PrettyStringTypes.Minimum)}");
                    return this.InternalUnify(
                        context, unification1.Expression, placeholder2, direction, raiseCouldNotUnify);
                case (false, true):
                    Debug.WriteLine($"Substitute2: {placeholder1.GetPrettyString(PrettyStringTypes.Minimum)} --> {placeholder2.GetPrettyString(PrettyStringTypes.Minimum)} [{unification2}]");
                    return this.InternalUnify(
                        context, placeholder1, unification2.Expression, direction, raiseCouldNotUnify);
                default:
                    Debug.WriteLine($"Substitute2: {placeholder1.GetPrettyString(PrettyStringTypes.Minimum)} --> {placeholder2.GetPrettyString(PrettyStringTypes.Minimum)}");
                    context.Set(placeholder1, placeholder2, direction);
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
                // Function unification.
                case (ILambdaExpression({ } fp, { } fr),
                      ILambdaExpression({ } tp, { } tr),
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
                        return context.Commit(rp && rr, raiseCouldNotUnify);
                    }
                
                // TODO: IApplyExpression (applicable type functions)
                
                // Placeholder unification.
                case (IPlaceholderTerm fph, IPlaceholderTerm tph, _):
                    return this.Substitute2(
                        context, fph, tph, direction, raiseCouldNotUnify);
                case (IPlaceholderTerm fph, _, UnifyDirections.BiDirectional):
                    return this.Substitute(
                        context, fph, expression2, UnifyDirections.BiDirectional, raiseCouldNotUnify);
                case (_, IPlaceholderTerm tph, UnifyDirections.BiDirectional):
                    return this.Substitute(
                        context, tph, expression1, UnifyDirections.BiDirectional, raiseCouldNotUnify);
                case (IPlaceholderTerm fph, _, UnifyDirections.Forward):
                    return this.Substitute(
                        context, fph, expression2, UnifyDirections.Forward, raiseCouldNotUnify);
                case (IPlaceholderTerm fph, _, UnifyDirections.Backward):
                    return this.Substitute(
                        context, fph, expression2, UnifyDirections.Backward, raiseCouldNotUnify);
                case (_, IPlaceholderTerm tph, UnifyDirections.Forward):
                    return this.Substitute(
                        context, tph, expression1, UnifyDirections.Backward, raiseCouldNotUnify);
                case (_, IPlaceholderTerm tph, UnifyDirections.Backward):
                    return this.Substitute(
                        context, tph, expression1, UnifyDirections.Forward, raiseCouldNotUnify);
                
                // Binary expression unification.
                case (IAndExpression fa, _, _):
                    using (context.BeginScope())
                    {
                        var rfl = this.InternalUnify(
                            context, fa.Left, expression2, direction, raiseCouldNotUnify);
                        var rfr = this.InternalUnify(
                            context, fa.Right, expression2, direction, raiseCouldNotUnify);
                        return context.Commit(rfl && rfr, raiseCouldNotUnify);
                    }
                case (_, IAndExpression ta, _):
                    using (context.BeginScope())
                    {
                        var rtl = this.InternalUnify(
                            context, expression1, ta.Left, direction, raiseCouldNotUnify);
                        var rtr = this.InternalUnify(
                            context, expression1, ta.Right, direction, raiseCouldNotUnify);
                        return context.Commit(rtl && rtr, raiseCouldNotUnify);
                    }
                case (IOrExpression fo, _, _):
                    using (context.BeginScope())
                    {
                        var rfl = this.InternalUnify(
                            context, fo.Left, expression2, direction, false);
                        var rfr = this.InternalUnify(
                            context, fo.Right, expression2, direction, false);
                        return context.Commit(rfl || rfr, raiseCouldNotUnify);
                    }
                case (_, IOrExpression to, _):
                    using (context.BeginScope())
                    {
                        var rtl = this.InternalUnify(
                            context, expression1, to.Left, direction, false);
                        var rtr = this.InternalUnify(
                            context, expression1, to.Right, direction, false);
                        return context.Commit(rtl || rtr, raiseCouldNotUnify);
                    }

                // Validate polarity.
                case (_, _, UnifyDirections.Forward):
                    // check(expression1 <: expression2)
                    if (!context.IsAssignableFrom(expression1, expression2))
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
                    // check(expression1 :> expression2)
                    if (!context.IsAssignableFrom(expression2, expression1))
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
                    // check(expression1 <:> expression2)
                    if (!context.IsAssignable(expression1, expression2))
                    {
                        if (raiseCouldNotUnify)
                        {
                            throw new ArgumentException(
                                $"Couldn't unify: {expression1.GetPrettyString(PrettyStringTypes.Minimum)} <:> {expression2.GetPrettyString(PrettyStringTypes.Minimum)}");
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
                    return context.Commit(false, raiseCouldNotUnify);
                }

                // Unify if succeeded higher order.
                return context.Commit(
                    this.InternalUnifyCore(
                        context,
                        expression1,
                        expression2,
                        direction,
                        raiseCouldNotUnify),
                    raiseCouldNotUnify);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////

        [DebuggerStepThrough]
        public void Unify(
            UnifyContext context,
            IExpression expression1,
            IExpression expression2,
            bool bidirectional) =>
            this.InternalUnify(   // TODO: trap failure deeper unification.
                context, expression1, expression2,
                bidirectional ? UnifyDirections.BiDirectional : UnifyDirections.Forward,
                true);
        
        public static readonly Unifier Instance =
            new Unifier();
    }
}
