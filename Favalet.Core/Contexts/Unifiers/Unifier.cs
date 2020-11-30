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

namespace Favalet.Contexts.Unifiers
{
    internal sealed partial class Unifier
    {
        [DebuggerStepThrough]
        private Unifier()
        { }

        ///////////////////////////////////////////////////////////////////////////////////
        
        private bool InternalAdd(
            UnifyContext context,
            IPlaceholderTerm placeholder,
            IExpression expression,
            UnificationPolarities polarity)
        {
            if (context.GetOrAddNode(placeholder, expression, polarity, out var node) ==
                GetOrAddNodeResults.Added)
            {
                return true;
            }
            
            Debug.Assert(node.Unifications.Count >= 1);

            var removeCandidates = new List<Unification>();
            var succeeded = false;
            var append = false;
            foreach (var unification in node.Unifications)
            {
                var (ru, ra) = (unification.Polarity, polarity) switch
                {
                    // ph <=> u
                    // ph <== ex
                    (UnificationPolarities.Both, UnificationPolarities.In) =>
                        (this.InternalUnify(context, expression, unification.Expression, false, false), false),
                    // ph <=> u
                    // ph ==> ex
                    (UnificationPolarities.Both, UnificationPolarities.Out) =>
                        (this.InternalUnify(context, unification.Expression, expression, false, false), false),
                    // ph <== u
                    // ph <=> ex
                    (UnificationPolarities.In, UnificationPolarities.Both) =>
                        (this.InternalUnify(context, unification.Expression, expression, false, false), false),
                    // ph ==> u
                    // ph <=> ex
                    (UnificationPolarities.Out, UnificationPolarities.Both) =>
                        (this.InternalUnify(context, expression, unification.Expression, false, false), false),
                    // ph ==> u
                    // ph <== ex
                    (UnificationPolarities.Out, UnificationPolarities.In) =>
                        (this.InternalUnify(context, expression, unification.Expression, false, false), false),
                    // ph <== u
                    // ph ==> ex
                    (UnificationPolarities.In, UnificationPolarities.Out) =>
                        (this.InternalUnify(context, unification.Expression, expression, false, false), false),
                    // ph <== u
                    // ph <== ex
                    (UnificationPolarities.In, UnificationPolarities.In) =>
                        context.TypeCalculator.Equals(unification.Expression, expression) ?
                            (true, false) :
                            (true, true),
                    // ph ==> u
                    // ph ==> ex
                    (UnificationPolarities.Out, UnificationPolarities.Out) =>
                        context.TypeCalculator.Equals(unification.Expression, expression) ?
                            (true, false) :
                            (true, true),
                    // ph <=> u
                    // ph <=> ex
                    _ =>
                        (this.InternalUnify(context, unification.Expression, expression, true, false), false)
                };

                switch (ru, ra)
                {
                    case (true, false):
                        //ru.Finish();
                        succeeded = true;
                        break;
                    case (false, false):
                        removeCandidates.Add(unification);
                        break;
                    case (true, true):
                        succeeded = true;
                        append = true;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            foreach (var unification in removeCandidates)
            {
                node.Unifications.Remove(unification);
            }

            if (!succeeded || append)
            {
                node.Unifications.Add(Unification.Create(expression, polarity));
            }

            return true;
        }

        private bool AddBoth(
            UnifyContext context,
            IExpression from,
            IExpression to)
        {
            var fr = (from is IPlaceholderTerm fph) ?
                this.InternalAdd(
                    context,
                    fph,
                    to,
                    UnificationPolarities.Both) :
                true;

            var tr = (to is IPlaceholderTerm tph) ?
                this.InternalAdd(
                    context,
                    tph,
                    from,
                    UnificationPolarities.Both) :
                true;

            return fr && tr;
        }

        private bool AddForward(
            UnifyContext context,
            IPlaceholderTerm placeholder,
            IExpression from)
        {
            var fr = this.InternalAdd(
                context,
                placeholder,
                from,
                UnificationPolarities.In);
            
            var tr = (from is IPlaceholderTerm ei) ?
                this.InternalAdd(
                    context,
                    ei,
                    placeholder,
                    UnificationPolarities.Out) :
                true;

            return fr && tr;
        }

        private bool AddBackward(
            UnifyContext context,
            IPlaceholderTerm placeholder,
            IExpression to)
        {
            var tr = this.InternalAdd(
                context,
                placeholder,
                to,
                UnificationPolarities.Out);

            var fr = (to is IPlaceholderTerm ei) ?
                this.InternalAdd(
                    context,
                    ei,
                    placeholder,
                    UnificationPolarities.In) :
                true;

            return tr && fr;
        }

        ///////////////////////////////////////////////////////////////////////////////////
        
        private bool InternalUnifyCore(
            UnifyContext context,
            IExpression from,
            IExpression to,
            bool bidirectional,
            bool raiseIfCouldNotUnify)
        {
            Debug.Assert(!(from is IIgnoreUnificationTerm));
            Debug.Assert(!(to is IIgnoreUnificationTerm));

            switch (from, to, bidirectional)
            {
                // Binary expression unification.
                case (IBinaryExpression fb, _, _):
                    var br1 = this.InternalUnify(context, fb.Left, to, false, raiseIfCouldNotUnify);
                    var br2 = this.InternalUnify(context, fb.Right, to, false, raiseIfCouldNotUnify);
                    return br1 && br2;
                case (_, IBinaryExpression tb, _):
                    var br3 = this.InternalUnify(context, from, tb.Left, false, raiseIfCouldNotUnify);
                    var br4 = this.InternalUnify(context, from, tb.Right, false, raiseIfCouldNotUnify);
                    return br3 && br4;

                // Applied function unification.
                case (IFunctionExpression(IExpression fp, IExpression fr),
                      IAppliedFunctionExpression(IExpression tp, IExpression tr),
                      _):
                    // unify(C +> A): But parameters aren't binder.
                    var fr1 = this.InternalUnify(context, tp, fp, false, raiseIfCouldNotUnify);
                    // unify(B +> D)
                    var fr2 = this.InternalUnify(context, fr, tr, false, raiseIfCouldNotUnify);
                    return fr1 && fr2;

                // Function unification.
                case (IFunctionExpression(IExpression fp, IExpression fr),
                      IFunctionExpression(IExpression tp, IExpression tr),
                      _):
                    // unify(C +> A): Parameters are binder.
                    var fr3 = this.InternalUnify(context, tp, fp, false, raiseIfCouldNotUnify);
                    // unify(B +> D)
                    var fr4 = this.InternalUnify(context, fr, tr, false, raiseIfCouldNotUnify);
                    return fr3 && fr4;
                
                // TODO: IApplyExpression (applicable type functions)
                
                // Placeholder unification.
                case (_, IPlaceholderTerm tph, false):
                    return this.AddForward(context, tph, from);
                case (IPlaceholderTerm fph, _, false):
                    return this.AddBackward(context, fph, to);
                case (_, IPlaceholderTerm tph, true):
                    return this.AddBoth(context, tph, from);
                case (IPlaceholderTerm fph, _, true):
                    return this.AddBoth(context, fph, to);
            }
            
            // Validate polarity.
            // from <: to
            var f = context.TypeCalculator.Calculate(
                OrExpression.Create(from, to, TextRange.Unknown));
            if (!context.TypeCalculator.Equals(f, to))
            {
                if (raiseIfCouldNotUnify)
                {
                    throw new ArgumentException(
                        $"Couldn't unify: {from.GetPrettyString(PrettyStringTypes.Minimum)} <: {to.GetPrettyString(PrettyStringTypes.Minimum)}");
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        ///////////////////////////////////////////////////////////////////////////////////

        private bool InternalUnify(
            UnifyContext context,
            IExpression from,
            IExpression to,
            bool bidirectional,
            bool raiseIfCouldNotUnify)
        {
            // Same as.
            if (context.TypeCalculator.ExactEquals(from, to))
            {
                return true;
            }

            switch (from, to)
            {
                // Ignore IIgnoreUnificationTerm unification.
                case (IIgnoreUnificationTerm _, _):
                case (_, IIgnoreUnificationTerm _):
                    return true;
            }

            // Unify higher order.
            if (!this.InternalUnify(
                context,
                from.HigherOrder,
                to.HigherOrder,
                bidirectional,
                raiseIfCouldNotUnify))
            {
                return false;
            }

            // Unify if succeeded higher order.
            return this.InternalUnifyCore(
                context,
                from,
                to,
                bidirectional,
                raiseIfCouldNotUnify);
        }

        ///////////////////////////////////////////////////////////////////////////////////

        [DebuggerStepThrough]
        public void Unify(
            UnifyContext context,
            IExpression from,
            IExpression to,
            bool bidirectional) =>
            this.InternalUnify(context, from, to, bidirectional, true);
        
        public static readonly Unifier Instance =
            new Unifier();
    }
}
