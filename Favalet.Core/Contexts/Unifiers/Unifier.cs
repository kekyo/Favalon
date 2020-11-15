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

        private readonly struct UnifyResult
        {
            private static readonly Action empty = () => { };
            
            private readonly Action? finish;

            private UnifyResult(Action? finish) =>
                this.finish = finish;

            public bool IsSucceeded =>
                this.finish != null;

            public UnifyResult Finish()
            {
                if (this.finish != null)
                {
                    this.finish!();
                    return Succeeded();
                }
                else
                {
                    return Failed();
                }
            }

            public static UnifyResult operator &(UnifyResult lhs, UnifyResult rhs) =>
                (lhs.finish, rhs.finish) switch
                {
                    (Action l, Action r) => new UnifyResult(() => { l(); r(); }),
                    _ => UnifyResult.Failed()
                };
            
            public static UnifyResult Succeeded() =>
                new UnifyResult(empty);
            public static UnifyResult Succeeded(Action finish) =>
                new UnifyResult(finish);
            public static UnifyResult Failed() =>
                new UnifyResult(null);
        }

        private UnifyResult InternalUnifyCore(
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
                    var br1 = this.InternalUnify(fb.Left, to, false, raiseIfCouldNotUnify);
                    var br2 = this.InternalUnify(fb.Right, to, false, raiseIfCouldNotUnify);
                    return br1 & br2;
                case (_, IBinaryExpression tb, _):
                    var br3 = this.InternalUnify(from, tb.Left, false, raiseIfCouldNotUnify);
                    var br4 = this.InternalUnify(from, tb.Right, false, raiseIfCouldNotUnify);
                    return br3 & br4;

                // Applied function unification.
                case (IFunctionExpression(IExpression fp, IExpression fr),
                      IAppliedFunctionExpression(IExpression tp, IExpression tr),
                      _):
                    // unify(C +> A): But parameters aren't binder.
                    var fr1 = this.InternalUnify(tp, fp, false, raiseIfCouldNotUnify);
                    // unify(B +> D)
                    var fr2 = this.InternalUnify(fr, tr, false, raiseIfCouldNotUnify);
                    return (fr1 & fr2).Finish();

                // Function unification.
                case (IFunctionExpression(IExpression fp, IExpression fr),
                      IFunctionExpression(IExpression tp, IExpression tr),
                      _):
                    // unify(C +> A): Parameters are binder.
                    var fr3 = this.InternalUnify(tp, fp, false, raiseIfCouldNotUnify);
                    // unify(B +> D)
                    var fr4 = this.InternalUnify(fr, tr, false, raiseIfCouldNotUnify);
                    return (fr3 & fr4).Finish();
                
                // TODO: IApplyExpression (applicable type functions)
                
                // Placeholder unification.
                case (_, IPlaceholderTerm tph, false):
                    return this.AddForward(tph, from);
                case (IPlaceholderTerm fph, _, false):
                    return this.AddBackward(fph, to);
                case (_, IPlaceholderTerm tph, true):
                    return this.AddBoth(tph, from);
                case (IPlaceholderTerm fph, _, true):
                    return this.AddBoth(fph, to);
            }
            
            if (raiseIfCouldNotUnify)
            {
                // Validate polarity.
                // from <: to
                var f = this.TypeCalculator.Calculate(
                    OrExpression.Create(from, to, TextRange.Unknown));
                if (!this.TypeCalculator.Equals(f, to))
                {
                    throw new ArgumentException(
                        $"Couldn't unify: {from.GetPrettyString(PrettyStringTypes.Minimum)} <: {to.GetPrettyString(PrettyStringTypes.Minimum)}");
                }
                return UnifyResult.Succeeded();
            }

            return UnifyResult.Failed();
        }

        private UnifyResult InternalUnify(
            IExpression from,
            IExpression to,
            bool bidirectional,
            bool raiseIfCouldNotUnify)
        {
            // Same as.
            if (this.TypeCalculator.ExactEquals(from, to))
            {
                return UnifyResult.Succeeded();
            }

            switch (from, to)
            {
                // Ignore IIgnoreUnificationTerm unification.
                case (IIgnoreUnificationTerm _, _):
                case (_, IIgnoreUnificationTerm _):
                    return UnifyResult.Succeeded();

                default:
                    // Unify higher order.
                    var hr = this.InternalUnify(
                        from.HigherOrder,
                        to.HigherOrder,
                        bidirectional,
                        raiseIfCouldNotUnify);
                    if (hr.IsSucceeded)
                    {
                        // Unify if succeeded higher order.
                        var r = this.InternalUnifyCore(
                            from,
                            to,
                            bidirectional,
                            raiseIfCouldNotUnify);
                        return hr & r;
                    }
                    break;
            }

            return UnifyResult.Failed();
        }

        [DebuggerStepThrough]
        public void Unify(
            IExpression from,
            IExpression to,
            bool bidirectional) =>
            this.InternalUnify(from, to, bidirectional, true).
            Finish();

        [DebuggerStepThrough]
        public override string ToString() =>
            "Unifier: " + this.View;
        
        [DebuggerStepThrough]
        public static Unifier Create(ITypeCalculator typeCalculator, IExpression targetRoot) =>
            new Unifier(typeCalculator, targetRoot);
    }
}
