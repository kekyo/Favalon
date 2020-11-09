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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Favalet.Expressions;
using Favalet.Expressions.Algebraic;
using Favalet.Expressions.Specialized;
using Favalet.Internal;
using Favalet.Ranges;

namespace Favalet.Contexts.Unifiers
{
    public interface ITopology
    {
        string View { get; }    
        string Dot { get; }    
    }
    
    partial class Unifier
    {
        [DebuggerStepThrough]
        private sealed class Node
        {
            public readonly IPlaceholderTerm Placeholder;
            public readonly HashSet<Unification> Unifications = new HashSet<Unification>();

            public Node(IPlaceholderTerm placeholder) =>
                this.Placeholder = placeholder;

            public override string ToString() =>
                $"{this.Placeholder.GetPrettyString(PrettyStringTypes.Readable)} [" + 
                StringUtilities.Join(
                    ",",
                    this.Unifications.Select(unification => unification.ToString())) + "]";
        }
        
        private readonly Dictionary<IPlaceholderTerm, Node> topology =
            new Dictionary<IPlaceholderTerm, Node>(IdentityTermComparer.Instance);
        private Dictionary<IPlaceholderTerm, IPlaceholderTerm>? aliases;

        private bool InternalAdd(
            IPlaceholderTerm placeholder,
            IExpression expression,
            UnificationPolarities polarity)
        {
            if (!this.topology.TryGetValue(placeholder, out var node))
            {
                node = new Node(placeholder);
                this.topology.Add(placeholder, node);
            }

            var unification = Unification.Create(expression, polarity);
            return node.Unifications.Add(unification);
        }

        public void AddBoth(
            IExpression from,
            IExpression to)
        {
            if (from is IPlaceholderTerm fph)
            {
                this.InternalAdd(
                    fph,
                    to,
                    UnificationPolarities.Both);
            }

            if (to is IPlaceholderTerm tph)
            {
                this.InternalAdd(
                    tph,
                    from,
                    UnificationPolarities.Both);
            }
        }

        public void AddForward(
            IPlaceholderTerm placeholder,
            IExpression from)
        {
            this.InternalAdd(
                placeholder,
                from,
                UnificationPolarities.In);
            
            if (from is IPlaceholderTerm ei)
            {
                this.InternalAdd(
                    ei,
                    placeholder,
                    UnificationPolarities.Out);
            }
        }

        public void AddBackward(
            IPlaceholderTerm placeholder,
            IExpression to)
        {
            this.InternalAdd(
                placeholder,
                to,
                UnificationPolarities.Out);
            
            if (to is IPlaceholderTerm ei)
            {
                this.InternalAdd(
                    ei,
                    placeholder,
                    UnificationPolarities.In);
            }
        }

        private IPlaceholderTerm? GetAlias(IPlaceholderTerm placeholder, IPlaceholderTerm? defaultValue)
        {
            if (this.aliases == null)
            {
                return null;
            }
            return this.aliases.TryGetValue(placeholder, out var normalized) ?
                this.GetAlias(normalized, normalized) :
                defaultValue;
        }

        // Choicer drives with the topology knowledge.
        // This is inserting relations for between placeholders.
        private sealed class TypeTopologyChoicer :
            IExpressionChoicer
        {
            private readonly Unifier parent;

            public TypeTopologyChoicer(Unifier parent) =>
                this.parent = parent;
            
            public ChoiceResults ChoiceForAnd(
                ILogicalCalculator calculator,
                IExpressionChoicer self,
                IExpression left, IExpression right)
            {
                return this.parent.TypeCalculator.DefaultChoicer.ChoiceForAnd(
                    calculator, self, left, right);
            }

            public ChoiceResults ChoiceForOr(
                ILogicalCalculator calculator,
                IExpressionChoicer self,
                IExpression left, IExpression right)
            {
                return this.parent.TypeCalculator.DefaultChoicer.ChoiceForOr(
                    calculator, self, left, right);
            }
        }
            
        public void CalculateUnifications()
        {
            // Step 1-1: Generate alias dictionary.
            var preExtracts = this.topology.Select(entry =>
                    (placeholder: entry.Key,
                        aliases: entry.Value.Unifications.Where(unification =>
                            // Both placeholder unification
                            (unification.Polarity == UnificationPolarities.Both) &&
                            unification.Expression is IPlaceholderTerm))).SelectMany(entry =>
                    entry.aliases.Select(alias => (alias: (IPlaceholderTerm) alias.Expression, entry.placeholder)))
                .ToArray();

            var validKeys = new HashSet<IPlaceholderTerm>(
                preExtracts.Select(entry => entry.placeholder));
            
            this.aliases =
                preExtracts.Where(entry => !validKeys.Contains(entry.alias)).
                ToDictionary(entry => entry.alias, entry => entry.placeholder);

            // Step 1-2: Aggregate all aliased unifications.
            foreach (var (placeholder, node) in this.topology.ToArray())
            {
                // If this placeholder aliasing?
                if (this.GetAlias(placeholder, default) is IPlaceholderTerm normalized &&
                    this.topology.TryGetValue(normalized, out var targetNode))
                {
                    // Aggregate all unifications.
                    foreach (var unification in node.Unifications)
                    {
                        targetNode.Unifications.Add(unification);
                    }
                    
                    // Remove this placeholder from topology.
                    this.topology.Remove(placeholder);
                }
            }
            
            // Step 1-3: Resolve aliases on all unifications.
            foreach (var unification in this.topology.Values.
                SelectMany(node => node.Unifications))
            {
                IExpression Replacer(IExpression expression) =>
                    expression switch
                    {
                        IPlaceholderTerm placeholder =>
                            this.GetAlias(placeholder, placeholder)!,
                        IPairExpression(IExpression left, IExpression right) pair =>
                            pair.Create(Replacer(left), Replacer(right), pair.Range),
                        _ => expression
                    };
                var updated = Replacer(unification.Expression);
                unification.UpdateExpression(updated);
            }

            // Step 2: Calculate and aggregate all unifications each nodes.
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
        }

        #region Resolve
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
            if (this.topology.TryGetValue(placeholder, out var node))
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
                                parent.Range);
                        default:
                            return expression;
                    }
                }
        
                var expressions = node.Unifications.
                    Where(unification =>
                        (unification.Polarity == context.Polarity) ||
                        (unification.Polarity == UnificationPolarities.Both)).
                    Select(unification => ResolveRecursive(unification.Expression)).
                    ToArray();
                if (expressions.Length >= 1)
                {
                    // TODO: placeholder filtering idea is bad?
                    if (expressions.All(expression => expression.IsContainsPlaceholder))
                    {
                        var calculated = context.Compute(
                            expressions, TextRange.Unknown)!;   // TODO: range
                        return calculated;
                    }
                    else
                    {
                        var filtered = expressions.
                            Where(expression => !expression.IsContainsPlaceholder).
                            ToArray();
                        var calculated = context.Compute(
                            filtered, TextRange.Unknown)!;   // TODO: range
                        return calculated;
                    }
                }
            }

            return placeholder;
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
        #endregion
        
        #region Validator
        [DebuggerStepThrough]
        private void Validate(PlaceholderMarker marker, IExpression expression)
        {
            if (expression is IPlaceholderTerm placeholder)
            {
                this.Validate(marker, placeholder);
            }
            else if (expression is IPairExpression parent)
            {
                this.Validate(marker.Fork(), parent.Left);
                this.Validate(marker.Fork(), parent.Right);
            }
        }

        [DebuggerStepThrough]
        private void Validate(PlaceholderMarker marker, IPlaceholderTerm placeholder)
        {
            var targetPlaceholder = placeholder;
            while (true)
            {
                if (marker.Mark(targetPlaceholder))
                {
                    if (this.topology.TryGetValue(targetPlaceholder, out var node))
                    {
                        if (node.Unifications is IPlaceholderTerm pnext)
                        {
                            targetPlaceholder = pnext;
                            continue;
                        }
                        else
                        {
                            Validate(marker, (IIdentityTerm)node.Unifications);
                        }
                    }

                    return;
                }
#if DEBUG
                Debug.WriteLine(
                    "Detected circular variable reference: " + marker);
                throw new InvalidOperationException(
                    "Detected circular variable reference: " + marker);
#else
                throw new InvalidOperationException(
                    "Detected circular variable reference: " + marker);
#endif
            }
        }
        
        [DebuggerStepThrough]
        public void Validate(IPlaceholderTerm placeholder) =>
            this.Validate(PlaceholderMarker.Create(), placeholder);
        #endregion

        public string View
        {
            [DebuggerStepThrough]
            get => StringUtilities.Join(
                System.Environment.NewLine,
                this.topology.
                    // TODO: alias
                    OrderBy(entry => entry.Key, IdentityTermComparer.Instance).
                    SelectMany(entry =>
                        entry.Value.Unifications.Select(unification =>
                            $"{entry.Key.Symbol} {unification.ToString(PrettyStringTypes.Minimum)}")));
        }

        public string Dot
        {
            [DebuggerStepThrough]
            get
            {
                var tw = new StringWriter();
                tw.WriteLine("digraph topology");
                tw.WriteLine("{");
#if DEBUG
                tw.WriteLine(
                    "    graph [label=\"{0}\"];",
                    this.targetRoot.GetPrettyString(PrettyStringTypes.ReadableAll));
#else
                tw.WriteLine(
                    "    graph [label=\"{0}\"];",
                    this.targetRootString);
#endif
                tw.WriteLine();
                tw.WriteLine("    # nodes");

                var symbolMap = new Dictionary<IExpression, string>();
                
                (string symbol, IExpression expression) ToSymbolString(IExpression expression)
                {
                    switch (expression)
                    {
                        case IPlaceholderTerm ph:
                            return ($"ph{ph.Index}", expression);
                        case IPairExpression parent:
                            if (!symbolMap!.TryGetValue(parent, out var symbol2))
                            {
                                var index = symbolMap.Count;
                                symbol2 = $"pe{index}";
                                symbolMap.Add(parent, symbol2);
                            }
                            return (symbol2, parent);
                        default:
                            if (!symbolMap!.TryGetValue(expression, out var symbol1))
                            {
                                var index = symbolMap.Count;
                                symbol1 = $"ex{index}";
                                symbolMap.Add(expression, symbol1);
                            }
                            return (symbol1, expression);
                    }
                }

                foreach (var entry in this.topology.
                    Select(entry => (ph: (IIdentityTerm)entry.Key, label: entry.Key.Symbol)).
                    Concat((this.aliases != null) ? this.aliases.Keys.
                        Select(key => (ph: (IIdentityTerm)key, label: key.Symbol)) : 
                        Enumerable.Empty<(IIdentityTerm ph, string label)>()).
                    Concat((this.aliases != null) ? this.aliases.Values.
                        OfType<IIdentityTerm>().
                        Select(value => (ph: value, label: value.Symbol)) :
                        Enumerable.Empty<(IIdentityTerm ph, string label)>()).
                    Distinct().
                    OrderBy(entry => entry.ph, IdentityTermComparer.Instance))
                {
                    tw.WriteLine(
                        "    {0} [label=\"{1}\",shape=circle];",
                        ToSymbolString(entry.ph).symbol,
                        entry.label);
                }

                foreach (var entry in this.topology.
                    SelectMany(entry =>
                        entry.Value.Unifications.
                        Where(unification => !(unification.Expression is IPlaceholderTerm)).
                        Select(unification => ToSymbolString(unification.Expression))).
                    Distinct().
                    OrderBy(entry => entry.symbol))
                {
                    tw.WriteLine(
                        "    {0} [{1}];",
                        entry.symbol,
                        entry.expression switch
                        {
                            IPairExpression parent =>
                                $"xlabel=\"{parent.GetPrettyString(PrettyStringTypes.Minimum)}\",label=\"<i0>[0]\",shape=record",
                            _ =>
                                $"label=\"{entry.expression.GetPrettyString(PrettyStringTypes.Minimum)}\",shape=box",
                        });
                }

                tw.WriteLine();
                tw.WriteLine("    # topology");

                IEnumerable<(string from, string to, string attribute)> ToSymbols(IPlaceholderTerm placeholder, Unification unification)
                {
                    var phSymbol = ToSymbolString(placeholder).symbol;
                    switch (unification.Polarity, unification.Expression)
                    {
                        case (UnificationPolarities.Out, IPairExpression parent):
                            yield return (phSymbol, $"{ToSymbolString(parent).symbol}:i0", "");
                            yield return (phSymbol, $"{ToSymbolString(parent).symbol}:i1", "");
                            break;
                        case (UnificationPolarities.In, IPairExpression parent):
                            yield return ($"{ToSymbolString(parent).symbol}:i0", phSymbol, "");
                            yield return ($"{ToSymbolString(parent).symbol}:i1", phSymbol, "");
                            break;
                        case (UnificationPolarities.Both, IPairExpression parent):
                            yield return ($"{ToSymbolString(parent).symbol}:i0", phSymbol, " [dir=none]");
                            yield return ($"{ToSymbolString(parent).symbol}:i1", phSymbol, " [dir=none]");
                            break;
                        case (UnificationPolarities.Out, _):
                            yield return (phSymbol, ToSymbolString(unification.Expression).symbol, "");
                            break;
                        case (UnificationPolarities.In, _):
                            yield return (ToSymbolString(unification.Expression).symbol, phSymbol, "");
                            break;
                        case (UnificationPolarities.Both, _):
                            yield return (ToSymbolString(unification.Expression).symbol, phSymbol, " [dir=none]");
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
                    
                foreach (var entry in this.topology.
                    SelectMany(entry => entry.Value.Unifications.
                        SelectMany(unification => ToSymbols(entry.Key, unification))).
                    Distinct().
                    OrderBy(entry => entry.Item1))
                {
                    tw.WriteLine(
                        "    {0} -> {1}{2};",
                        entry.from,
                        entry.to,
                        entry.attribute);
                }
                
                tw.WriteLine();
                tw.WriteLine("    # aliases");

                foreach (var entry in this.aliases.
                    Select(entry =>
                    {
                        var resolved = this.GetAlias(entry.Key, entry.Key)!;
                        return
                            (from: ToSymbolString(entry.Key).symbol,
                               to: ToSymbolString(resolved).symbol);
                    }).
                    Distinct().
                    OrderBy(entry => entry.from))
                {
                    tw.WriteLine(
                        "    {0} -> {1} [dir=none];",
                        entry.from,
                        entry.to);
                }

                tw.WriteLine("}");

                return tw.ToString();
            }
        }
    }
}
