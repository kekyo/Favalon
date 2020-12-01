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

namespace Favalet.Contexts.Unifiers
{
    internal sealed partial class Unifier
    {
        [DebuggerStepThrough]
        private Unifier()
        { }

        ///////////////////////////////////////////////////////////////////////////////////

        private enum UnifyPartialResults
        {
            Left,
            Right,
            Both
        }
        
        private bool InternalAdd(
            UnifyContext context,
            IPlaceholderTerm placeholder,
            IExpression expression,
            UnificationPolarities polarity,
            bool raiseCouldNotUnify)
        {
            if (context.GetOrAddNode(placeholder, expression, polarity, out var node) ==
                GetOrAddNodeResults.Added)
            {
                Debug.Assert(node.Unifications.Count >= 1);
                return true;
            }
            
            Debug.Assert(node.Unifications.Count >= 1);

            var requireRight = false;
            
            foreach (var unification in node.Unifications.ToArray())
            {
                UnifyPartialResults result;
                switch (unification.Polarity, polarity)
                {
                    // ph <=> u
                    // ph <== ex
                    case (UnificationPolarities.Both, UnificationPolarities.In):
                        this.InternalUnify(context, expression, unification.Expression, false, false);
                        result = UnifyPartialResults.Left;
                        break;
                    // ph <=> u
                    // ph ==> ex
                    case (UnificationPolarities.Both, UnificationPolarities.Out):
                        this.InternalUnify(context, unification.Expression, expression, false, false);
                        result = UnifyPartialResults.Left;
                        break;
                    // ph <== u
                    // ph <=> ex
                    case (UnificationPolarities.In, UnificationPolarities.Both):
                        this.InternalUnify(context, unification.Expression, expression, false, false);
                        result = UnifyPartialResults.Right;
                        break;
                    // ph ==> u
                    // ph <=> ex
                    case (UnificationPolarities.Out, UnificationPolarities.Both):
                        this.InternalUnify(context, expression, unification.Expression, false, false);
                        result = UnifyPartialResults.Right;
                        break;
                    // ph ==> u
                    // ph <== ex
                    case (UnificationPolarities.Out, UnificationPolarities.In):
                        result = this.InternalUnify(context, expression, unification.Expression, false, false) ?
                            UnifyPartialResults.Right : UnifyPartialResults.Left;
                        break;
                    // ph <== u
                    // ph ==> ex
                    case (UnificationPolarities.In, UnificationPolarities.Out):
                        result = this.InternalUnify(context, unification.Expression, expression, false, false) ?
                            UnifyPartialResults.Left : UnifyPartialResults.Right;
                        break;
                    // ph <== u
                    // ph <== ex
                    case (UnificationPolarities.In, UnificationPolarities.In):
                        result = context.TypeCalculator.Equals(unification.Expression, expression) ?
                            UnifyPartialResults.Left : UnifyPartialResults.Both;
                        break;
                    // ph ==> u
                    // ph ==> ex
                    case (UnificationPolarities.Out, UnificationPolarities.Out):
                        result = context.TypeCalculator.Equals(unification.Expression, expression) ?
                            UnifyPartialResults.Left : UnifyPartialResults.Both;
                        break;
                    // ph <=> u
                    // ph <=> ex
                    default:
                        result = this.InternalUnify(context, unification.Expression, expression, true, false) ?
                            UnifyPartialResults.Left : UnifyPartialResults.Both;
                        break;
                };

                switch (result)
                {
                    case UnifyPartialResults.Right:
                        node.Unifications.Remove(unification);
                        requireRight = true;
                        break;
                    case UnifyPartialResults.Both:
                        requireRight = true;
                        break;
                }
            }

            if (requireRight)
            {
                node.Unifications.Add(Unification.Create(expression, polarity));
            }

            if (node.Unifications.Count >= 1)
            {
                return true;
            }

            if (raiseCouldNotUnify)
            {
                throw new ArgumentException(
                    $"Couldn't unify: {placeholder.GetPrettyString(PrettyStringTypes.Minimum)}, {expression.GetPrettyString(PrettyStringTypes.Minimum)}");
            }
            else
            {
                return false;
            }
        }

        private bool AddBoth(
            UnifyContext context,
            IExpression from,
            IExpression to,
            bool raiseCouldNotUnify)
        {
            using (context.AllScope())
            {
                switch (from, to)
                {
                    case (IPlaceholderTerm fph, IPlaceholderTerm tph):
                        context.AddAlias(fph, tph);
                        return true;
                    case (IPlaceholderTerm fph2, _):
                        return this.InternalAdd(context, fph2, to, UnificationPolarities.Both, raiseCouldNotUnify);
                    case (_, IPlaceholderTerm tph2):
                        return this.InternalAdd(context, tph2, from, UnificationPolarities.Both, raiseCouldNotUnify);
                }
            }

            throw new InvalidOperationException();
        }

        private bool AddForward(
            UnifyContext context,
            IPlaceholderTerm placeholder,
            IExpression from,
            bool raiseCouldNotUnify)
        {
            using (context.AllScope())
            {
                if (!this.InternalAdd(context, placeholder, from, UnificationPolarities.In, raiseCouldNotUnify))
                {
                    return false;
                }

                if (from is IPlaceholderTerm ei &&
                    !this.InternalAdd(context, ei, placeholder, UnificationPolarities.Out, raiseCouldNotUnify))
                {
                    return false;
                }
            }

            return true;
        }

        private bool AddBackward(
            UnifyContext context,
            IPlaceholderTerm placeholder,
            IExpression to,
            bool raiseCouldNotUnify)
        {
            using (context.AllScope())
            {
                if (!this.InternalAdd(context, placeholder, to, UnificationPolarities.Out, raiseCouldNotUnify))
                {
                    return false;
                }

                if (to is IPlaceholderTerm ei &&
                    this.InternalAdd(context, ei, placeholder, UnificationPolarities.In, raiseCouldNotUnify))
                {
                    return false;
                }
            }

            return true;
        }

        ///////////////////////////////////////////////////////////////////////////////////
        
        private bool InternalUnifyCore(
            UnifyContext context,
            IExpression from,
            IExpression to,
            bool bidirectional,
            bool raiseCouldNotUnify)
        {
            Debug.Assert(!(from is IIgnoreUnificationTerm));
            Debug.Assert(!(to is IIgnoreUnificationTerm));

            switch (from, to, bidirectional)
            {
                // Binary expression unification.
                case (IBinaryExpression fb, _, _):
                    var rfl = this.InternalUnify(context, fb.Left, to, false, raiseCouldNotUnify);
                    var rfr = this.InternalUnify(context, fb.Right, to, false, raiseCouldNotUnify);
                    return rfl && rfr;
                case (_, IBinaryExpression tb, _):
                    var rtl = this.InternalUnify(context, from, tb.Left, false, raiseCouldNotUnify);
                    var rtr = this.InternalUnify(context, from, tb.Right, false, raiseCouldNotUnify);
                    return rtl && rtr;

                // Applied function unification.
                case (IFunctionExpression(IExpression fp, IExpression fr),
                      IAppliedFunctionExpression(IExpression tp, IExpression tr),
                      _):
                    using (context.AllScope())
                    {
                        // unify(C +> A): But parameters aren't binder.
                        var rp = this.InternalUnify(context, tp, fp, false, raiseCouldNotUnify);
                        // unify(B +> D)
                        var rr = this.InternalUnify(context, fr, tr, false, raiseCouldNotUnify);
                        return rp && rr;
                    }

                // Function unification.
                case (IFunctionExpression(IExpression fp, IExpression fr),
                      IFunctionExpression(IExpression tp, IExpression tr),
                      _):
                    using (context.AllScope())
                    {
                        // unify(C +> A): Parameters are binder.
                        var rp = this.InternalUnify(context, tp, fp, false, raiseCouldNotUnify);
                        // unify(B +> D)
                        var rr = this.InternalUnify(context, fr, tr, false, raiseCouldNotUnify);
                        return rp && rr;
                    }
                
                // TODO: IApplyExpression (applicable type functions)
                
                // Placeholder unification.
                case (_, IPlaceholderTerm tph, false):
                    return this.AddForward(context, tph, from, raiseCouldNotUnify);
                case (IPlaceholderTerm fph, _, false):
                    return this.AddBackward(context, fph, to, raiseCouldNotUnify);
                case (_, IPlaceholderTerm tph, true):
                    return this.AddBoth(context, tph, from, raiseCouldNotUnify);
                case (IPlaceholderTerm fph, _, true):
                    return this.AddBoth(context, fph, to, raiseCouldNotUnify);
            }
            
            // Validate polarity.
            // from <: to
            var f = context.TypeCalculator.Calculate(
                OrExpression.Create(from, to, TextRange.Unknown));
            if (!context.TypeCalculator.Equals(f, to))
            {
                if (raiseCouldNotUnify)
                {
                    throw new ArgumentException(
                        $"Couldn't unify: {from.GetPrettyString(PrettyStringTypes.Minimum)} <: {to.GetPrettyString(PrettyStringTypes.Minimum)}");
                }
                else
                {
                    context.MarkCouldNotUnify(from, to);
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
            IExpression from,
            IExpression to,
            bool bidirectional,
            bool raiseCouldNotUnify)
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

            using (context.AllScope())
            {
                // Unify higher order.
                if (!this.InternalUnify(
                    context,
                    from.HigherOrder,
                    to.HigherOrder,
                    bidirectional,
                    raiseCouldNotUnify))
                {
                    return false;
                }

                // Unify if succeeded higher order.
                if (!this.InternalUnifyCore(
                    context,
                    from,
                    to,
                    bidirectional,
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
            IExpression from,
            IExpression to,
            bool bidirectional) =>
            this.InternalUnify(context, from, to, bidirectional, true);
        
        public static readonly Unifier Instance =
            new Unifier();
    }
}
