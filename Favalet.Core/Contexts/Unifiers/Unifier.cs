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
using Favalet.Ranges;

namespace Favalet.Contexts.Unifiers
{
    [DebuggerDisplay("{View}")]
    internal sealed partial class Unifier :
        FixupContext,  // Because used by simplist Dot property implementation.
        ITopology
    {
#if DEBUG
        private IExpression targetRoot;
#else
        private string targetRootString;
#endif
        
        [DebuggerStepThrough]
        private Unifier(ITypeCalculator typeCalculator, IExpression targetRoot) :
            base(typeCalculator)
        {
#if DEBUG
            this.targetRoot = targetRoot;
#else
            this.targetRootString =
                targetRoot.GetPrettyString(PrettyStringTypes.ReadableAll);
#endif
        }

        [DebuggerStepThrough]
        public void SetTargetRoot(IExpression targetRoot) =>
#if DEBUG
            this.targetRoot = targetRoot;
#else
            this.targetRootString =
                targetRoot.GetPrettyString(PrettyStringTypes.ReadableAll);
#endif

        private bool InternalUnifyCore(
            IExpression from,
            IExpression to,
            bool bidirectional,
            bool raiseIfCouldNotUnify)
        {
            Debug.Assert(!(from is IIgnoreUnificationTerm));
            Debug.Assert(!(to is IIgnoreUnificationTerm));

            switch (from, to, bidirectional, raiseIfCouldNotUnify)
            {
                // Placeholder unification.
                case (_, IPlaceholderTerm tph, false, _):
                    this.AddForward(tph, from);
                    //this.topology.Validate(tp2);
                    return true;
                case (IPlaceholderTerm fph, _, false, _):
                    this.AddBackward(fph, to);
                    //this.topology.Validate(fp2);
                    return true;
                case (_, IPlaceholderTerm tph, true, _):
                     this.AddBoth(tph, from);
                     //this.topology.Validate(tp2);
                     return true;
                case (IPlaceholderTerm fph, _, true, _):
                    this.AddBoth(fph, to);
                    //this.topology.Validate(fp2);
                    return true;

                // Binary expression unification.
                case (IBinaryExpression fb, _, _, _):
                    this.InternalUnify(fb.Left, to, false, false);
                    this.InternalUnify(fb.Right, to, false, false);
                    return true;
                case (_, IBinaryExpression tb, _, _):
                    this.InternalUnify(from, tb.Left, false, false);
                    this.InternalUnify(from, tb.Right, false, false);
                    return true;

                // Applied function unification.
                case (IFunctionExpression(IExpression fp, IExpression fr),
                      IAppliedFunctionExpression(IExpression tp, IExpression tr),
                      _, _):
                    // unify(C +> A): But parameters aren't binder.
                    this.InternalUnify(tp, fp, false, true);
                    // unify(B +> D)
                    this.InternalUnify(fr, tr, false, true);
                    return true;

                // Function unification.
                case (IFunctionExpression(IExpression fp, IExpression fr),
                      IFunctionExpression(IExpression tp, IExpression tr),
                      _, _):
                    // unify(C +> A): Parameters are binder.
                    this.InternalUnify(tp, fp, false, true);
                    // unify(B +> D)
                    this.InternalUnify(fr, tr, false, true);
                    return true;
                
                case (_, _, _, true):
                    // Validate polarity.
                    // from <: to
                    var f = this.TypeCalculator.Compute(
                        OrExpression.Create(from, to, TextRange.Unknown));
                    if (!this.TypeCalculator.Equals(f, to))
                    {
                        throw new ArgumentException(
                            $"Couldn't unify: {from.GetPrettyString(PrettyStringTypes.Minimum)} <: {to.GetPrettyString(PrettyStringTypes.Minimum)}");
                    }
                    return true;
            }

            return false;
        }

        private bool InternalUnify(
            IExpression from,
            IExpression to,
            bool bidirectional,
            bool raiseIfCouldNotUnify)
        {
            // Same as.
            if (this.TypeCalculator.ExactEquals(from, to))
            {
                return true;
            }

            switch (from, to)
            {
                // Ignore IIgnoreUnificationTerm unification.
                case (IIgnoreUnificationTerm _, _):
                case (_, IIgnoreUnificationTerm _):
                    return true;

                default:
                    // Unify higher order.
                    if (this.InternalUnify(
                        from.HigherOrder,
                        to.HigherOrder,
                        bidirectional,
                        raiseIfCouldNotUnify))
                    {
                        // Unify if succeeded higher order.
                        return this.InternalUnifyCore(
                            from,
                            to,
                            bidirectional,
                            raiseIfCouldNotUnify);
                    }
                    break;
            }

            return false;
        }

        [DebuggerStepThrough]
        public void Unify(
            IExpression from,
            IExpression to,
            bool bidirectional) =>
            this.InternalUnify(from, to, bidirectional, true);

        [DebuggerStepThrough]
        public override string ToString() =>
            "Unifier: " + this.View;
        
        [DebuggerStepThrough]
        public static Unifier Create(ITypeCalculator typeCalculator, IExpression targetRoot) =>
            new Unifier(typeCalculator, targetRoot);
    }
}
