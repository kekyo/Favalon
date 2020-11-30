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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Favalet.Contexts.Unifiers
{
    public interface ITopology
    {
        string View { get; }    
        string Dot { get; }    
    }
    
    internal enum GetOrAddNodeResults
    {
        Got,
        Added
    }

    [DebuggerDisplay("{View}")]
    internal sealed partial class UnifyContext :
        ITopology
    {
#if DEBUG
        private IExpression targetRoot;
#else
        private string targetRootString;
#endif

        private readonly Dictionary<IPlaceholderTerm, Node> topology =
            new Dictionary<IPlaceholderTerm, Node>(IdentityTermComparer.Instance);

        public readonly ITypeCalculator TypeCalculator;
        
        [DebuggerStepThrough]
        private UnifyContext(ITypeCalculator typeCalculator, IExpression targetRoot)
        {
            this.TypeCalculator = typeCalculator;
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

        public GetOrAddNodeResults GetOrAddNode(
            IPlaceholderTerm placeholder,
            IExpression expression,
            UnificationPolarities polarity,
            out Node node)
        {
            if (this.topology.TryGetValue(placeholder, out node!))
            {
                return GetOrAddNodeResults.Got;
            }

            node = Node.Create(placeholder);
            this.topology.Add(placeholder, node);
            node.Unifications.Add(Unification.Create(expression, polarity));
            return GetOrAddNodeResults.Added;
        }
        
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
                    Concat(this.aliases.Keys.
                        Select(key => (ph: (IIdentityTerm)key, label: key.Symbol))).
                    Concat(this.aliases.Values.
                        OfType<IIdentityTerm>().
                        Select(value => (ph: value, label: value.Symbol))).
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

        [DebuggerStepThrough]
        public override string ToString() =>
            "UnifyContext: " + this.View;

        [DebuggerStepThrough]
        public static UnifyContext Create(ITypeCalculator typeCalculator, IExpression targetRoot) =>
            new UnifyContext(typeCalculator, targetRoot);
    }
}