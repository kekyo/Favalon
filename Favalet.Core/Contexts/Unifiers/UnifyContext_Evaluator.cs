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
using Favalet.Expressions.Specialized;
using Favalet.Internal;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Favalet.Contexts.Unifiers
{
    partial class UnifyContext
    {
        [DebuggerStepThrough]
        private sealed class AliasPlaceholderPairComparer :
            IEqualityComparer<(IPlaceholderTerm ph0, IPlaceholderTerm ph1)>,
            IComparer<(IPlaceholderTerm ph0, IPlaceholderTerm ph1)>
        {
            private AliasPlaceholderPairComparer()
            { }

            public bool Equals(
                (IPlaceholderTerm ph0, IPlaceholderTerm ph1) x,
                (IPlaceholderTerm ph0, IPlaceholderTerm ph1) y) =>
                (x.ph0.Equals(y.ph0) && x.ph1.Equals(y.ph1)) ||
                (x.ph0.Equals(y.ph1) && x.ph1.Equals(y.ph0));

            public int GetHashCode((IPlaceholderTerm ph0, IPlaceholderTerm ph1) obj) =>
                obj.ph0.GetHashCode() ^ obj.ph1.GetHashCode();

            public int Compare(
                (IPlaceholderTerm ph0, IPlaceholderTerm ph1) x,
                (IPlaceholderTerm ph0, IPlaceholderTerm ph1) y) =>
                x.ph0.Index.CompareTo(y.ph0.Index);

            public static readonly AliasPlaceholderPairComparer Instance =
                new AliasPlaceholderPairComparer();
        }

        // Traverse AST and will replace all alias.
        [DebuggerStepThrough]
        private IExpression ReplaceAlias(IExpression expression)
        {
            switch (expression)
            {
                case IPlaceholderTerm ph:
                    return this.GetAlias(ph, ph)!;
                case IPairExpression(IExpression left, IExpression right) pair:
                    var lr = this.ReplaceAlias(left);
                    var rr = this.ReplaceAlias(right);
                    if (!object.ReferenceEquals(left, lr) ||
                        !object.ReferenceEquals(right, rr))
                    {
                        return pair.Create(
                            lr, rr, UnspecifiedTerm.Instance, pair.Range);
                    }
                    else
                    {
                        return pair;
                    }
                default:
                    return expression;
            };
        }
        
        // Evaluate topology.
        public void EvaluateTopology()
        {
            // Step 1-1: Generate alias dictionary.
            foreach (var entry in this.topology.
                Select(entry =>
                    (placeholder: entry.Key,
                     aliases: entry.Value.Unifications.Where(unification =>
                        // Both placeholder unification
                        (unification.Polarity == UnificationPolarities.Both) &&
                        unification.Expression is IPlaceholderTerm))).
                SelectMany(entry =>
                    entry.aliases.Select(
                        alias => (alias: (IPlaceholderTerm)alias.Expression, entry.placeholder))).
                OrderByDescending(entry => entry, AliasPlaceholderPairComparer.Instance).    // saves by minimal index
                Distinct(AliasPlaceholderPairComparer.Instance))
            {
                this.aliases[entry.Item2] = entry.Item1;
            }
            
            // Step 1-2: Aggregate all aliased unification.
            foreach (var (placeholder, node) in this.topology.ToArray())
            {
                // If this placeholder aliasing?
                if (this.GetAlias(placeholder, default) is IPlaceholderTerm normalized &&
                    this.topology.TryGetValue(normalized, out var targetNode))
                {
                    // Aggregate all unification except self.
                    foreach (var unification in node.Unifications.
                        Where(unification =>
                            !((unification.Polarity == UnificationPolarities.Both) &&
                              unification.Expression.Equals(normalized))))
                    {
                        targetNode.Unifications.Add(unification);
                    }
                                        
                    // Remove duplicate equality unification.
                    foreach (var unification in targetNode.Unifications.
                        ToArray().
                        Where(unification =>
                            ((unification.Polarity == UnificationPolarities.Both) &&
                             unification.Expression.Equals(placeholder))))
                    {
                        targetNode.Unifications.Remove(unification);
                    }

                    // Remove this placeholder from topology.
                    this.topology.Remove(placeholder);
                }
            }
            
            // Step 1-3: Resolve aliases on all unification.
            foreach (var (placeholder, node) in this.topology.ToArray())
            {
                foreach (var unification in node.Unifications.ToArray())
                {
                    node.Unifications.Remove(unification);

                    var updated = this.ReplaceAlias(unification.Expression);
                    if (!((unification.Polarity == UnificationPolarities.Both) &&
                        updated.Equals(placeholder)))
                    {
                        node.Unifications.Add(Unification.Create(updated, unification.Polarity));
                    }
                }

                if (node.Unifications.Count == 0)
                {
                    this.topology.Remove(placeholder);
                }
            }
#if false
            // Step 2: Calculate and aggregate all unification each nodes.
            // Type relation extends by the type topology choicer inserted at run time.
            var choicer = new TypeTopologyChoicer(this);
            foreach (var (placeholder, node) in this.topology)
            {
                var unification = node.Unifications.
                    Aggregate((ua, ub) =>
                    {
                        switch (ua.Polarity, ub.Polarity)
                        {
                            // ph <=> ua
                            // ph <== ub
                            // check: ua == (ua | ub) : widen
                            // result: ua
                            case (UnificationPolarities.Both, UnificationPolarities.In):
                                var r1 = this.TypeCalculator.Calculate(
                                    OrExpression.Create(ua.Expression, ub.Expression, TextRange.Unknown),
                                    choicer);
                                if (!this.TypeCalculator.Equals(r1, ua.Expression))
                                {
                                    throw new InvalidOperationException();
                                }
                                return ua;

                            // ph <=> ua
                            // ph ==> ub
                            // check: ua == (ua & ub) : narrow
                            // result: ua
                            case (UnificationPolarities.Both, UnificationPolarities.Out):
                                var r2 = this.TypeCalculator.Calculate(
                                    AndExpression.Create(ua.Expression, ub.Expression, TextRange.Unknown),
                                    choicer);
                                if (!this.TypeCalculator.Equals(r2, ua.Expression))
                                {
                                    throw new InvalidOperationException();
                                }
                                return ua;

                            // ph <== ua
                            // ph <=> ub
                            // check: ub == (ua | ub) : widen
                            // result: ub
                            case (UnificationPolarities.In, UnificationPolarities.Both):
                                var r3 = this.TypeCalculator.Calculate(
                                    OrExpression.Create(ua.Expression, ub.Expression, TextRange.Unknown),
                                    choicer);
                                if (!this.TypeCalculator.Equals(r3, ub.Expression))
                                {
                                    throw new InvalidOperationException();
                                }
                                return ub;

                            // ph <=> ua
                            // ph <== ub
                            // check: ub == (ua & ub) : narrow
                            // result: ub
                            case (UnificationPolarities.Out, UnificationPolarities.Both):
                                var r4 = this.TypeCalculator.Calculate(
                                    AndExpression.Create(ua.Expression, ub.Expression, TextRange.Unknown),
                                    choicer);
                                if (!this.TypeCalculator.Equals(r4, ub.Expression))
                                {
                                    throw new InvalidOperationException();
                                }
                                return ub;
                            
                            // ph ==> ua
                            // ph <== ub
                            // check: ub == (ua & ub) : narrow
                            // result: ub
                            case (UnificationPolarities.Out, UnificationPolarities.In):
                                var r5 = this.TypeCalculator.Calculate(
                                    AndExpression.Create(ua.Expression, ub.Expression, TextRange.Unknown),
                                    choicer);
                                if (!this.TypeCalculator.Equals(r5, ub.Expression))
                                {
                                    throw new InvalidOperationException();
                                }
                                return ub;
                            
                            // ph <== ua
                            // ph ==> ub
                            // check: ua == (ua & ub) : narrow
                            // result: ua
                            case (UnificationPolarities.In, UnificationPolarities.Out):
                                var r6 = this.TypeCalculator.Calculate(
                                    AndExpression.Create(ua.Expression, ub.Expression, TextRange.Unknown),
                                    choicer);
                                if (!this.TypeCalculator.Equals(r6, ua.Expression))
                                {
                                    throw new InvalidOperationException();
                                }
                                return ua;

                            // ph <== ua
                            // ph <== ub
                            // result: ua & ub
                            case (UnificationPolarities.In, UnificationPolarities.In):
                                var r7 = this.TypeCalculator.Calculate(
                                    AndExpression.Create(ua.Expression, ub.Expression, TextRange.Unknown),
                                    choicer);
                                return Unification.Create(r7, UnificationPolarities.In);

                            // ph ==> ua
                            // ph ==> ub
                            // result: ua | ub
                            case (UnificationPolarities.Out, UnificationPolarities.Out):
                                var r8 = this.TypeCalculator.Calculate(
                                    OrExpression.Create(ua.Expression, ub.Expression, TextRange.Unknown),
                                    choicer);
                                return Unification.Create(r8, UnificationPolarities.Out);

                            // ph <=> ua
                            // ph <=> ub
                            // result: ua & ub
                            case (UnificationPolarities.Both, UnificationPolarities.Both):
                                var r9 = this.TypeCalculator.Calculate(
                                    AndExpression.Create(ua.Expression, ub.Expression, TextRange.Unknown),
                                    choicer);
                                return Unification.Create(r9, UnificationPolarities.Both);

                            default:
                                throw new InvalidOperationException();
                        };
                    });
                node.Unifications.Clear();
                node.Unifications.Add(unification);
            }
#endif
        }
    }
}
