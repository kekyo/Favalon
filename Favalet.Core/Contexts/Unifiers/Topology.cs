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

namespace Favalet.Contexts.Unifiers
{
    public interface ITopology
    {
        string View { get; }    
        string Dot { get; }    
    }
    
    [DebuggerDisplay("{View}")]
    internal sealed class Topology :
        ITopology
    {
        [DebuggerStepThrough]
        private sealed class Node
        {
            public readonly HashSet<Unification> Unifications;

            public Node() =>
                this.Unifications = new HashSet<Unification>();

            public void Merge(Node node)
            {
                foreach (var unification in node.Unifications)
                {
                    this.Unifications.Add(unification);
                }
            }

            public override string ToString() =>
                "[" + StringUtilities.Join(",", this.Unifications.Select(unification => unification.ToString())) + "]";
        }
        
        private readonly Dictionary<IPlaceholderTerm, Node> topology =
            new Dictionary<IPlaceholderTerm, Node>(IdentityTermComparer.Instance);
        private readonly Dictionary<IPlaceholderTerm, IExpression> aliases =
            new Dictionary<IPlaceholderTerm, IExpression>(IdentityTermComparer.Instance);

#if DEBUG
        private IExpression targetRoot;
#else
        private string targetRootString;
#endif

        [DebuggerStepThrough]
        private Topology(IExpression targetRoot)
        {
#if DEBUG
            this.targetRoot = targetRoot;
#else
            this.targetRootString =
                targetRoot.GetPrettyString(PrettyStringTypes.ReadableAll);
#endif
        }

        [DebuggerStepThrough]
        private bool InternalAddNormalized(
            IPlaceholderTerm placeholder,
            IExpression expression,
            UnificationPolarities polarity)
        {
            if (!this.topology.TryGetValue(placeholder, out var node))
            {
                node = new Node();
                this.topology.Add(placeholder, node);
            }

            var unification = Unification.Create(expression, polarity);
            return node.Unifications.Add(unification);
        }

        [DebuggerStepThrough]
        private T? GetAlias<T>(
            IPlaceholderTerm placeholder,
            T? defaultValue)
            where T : class, IExpression =>
            this.aliases.TryGetValue(placeholder, out var alias) ?
                (alias is IPlaceholderTerm a ? 
                    this.GetAlias<T>(a, (T)alias) : 
                    ((alias as T) ?? defaultValue)) :
                defaultValue;
        
        [DebuggerStepThrough]
        private bool InternalAdd(
            IPlaceholderTerm placeholder,
            IExpression expression,
            UnificationPolarities polarity)
        {
            var ph =
                this.GetAlias(placeholder, placeholder)!;
            var ex = expression is IPlaceholderTerm exph ?
                this.GetAlias(exph, exph)! :
                expression;

            return this.InternalAddNormalized(ph, ex, polarity);
        }

        [DebuggerStepThrough]
        public void AddBoth(
            IExpression from,
            IExpression to)
        {
            switch (from, to)
            {
                case (IPlaceholderTerm fph, IPlaceholderTerm tph)
                    when !fph.Equals(tph):
                    switch (this.GetAlias(fph, default(IPlaceholderTerm)), this.GetAlias(tph, default(IPlaceholderTerm)))
                    {
                        case (IPlaceholderTerm faph, null):
                            if (!faph.Equals(to))
                            {
                                this.AddBoth(
                                    faph,
                                    to);
                            }
                            break;
                        case (null, IPlaceholderTerm taph):
                            if (!from.Equals(taph))
                            {
                                this.AddBoth(
                                    from,
                                    taph);
                            }
                            break;
                        case (null, null):
                            var formalDirection = IdentityTermComparer.Instance.Compare(fph, tph);
                            if (formalDirection > 0)
                            {
                                this.aliases.Add(fph, tph);
                            }
                            else if (formalDirection < 0)
                            {
                                this.aliases.Add(tph, fph);
                            }
                            break;
                    }
                    break;
                
                case (IPlaceholderTerm fph, _):
                    this.InternalAdd(
                        fph,
                        to,
                        UnificationPolarities.Both);
                    break;
                
                case (_, IPlaceholderTerm tph):
                    this.InternalAdd(
                        tph,
                        from,
                        UnificationPolarities.Both);
                    break;
            }
        }

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
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

        public void NormalizeAliases(ITypeCalculator calculator)
        {
            // Will make aliases normalized topology excepts outside PlaceholderTerm instances.
            
            // Step 1: Resolve and shrink placeholder aliases.
            foreach (var entry in this.aliases)
            {
                if (this.topology.TryGetValue(entry.Key, out var source) &&
                    entry.Value is IPlaceholderTerm target)
                {
                    // Already declared topology key:
                    if (this.topology.TryGetValue(target, out var destination))
                    {
                        // Merge into destination.
                        destination.Merge(source);
                    }
                    else
                    {
                        // Redeclare topology key.
                        this.topology.Add(target, source);
                    }
                    this.topology.Remove(entry.Key);
                }
            }

            // Step 2: Resolve inside unification expressions.
            foreach (var entry in this.topology)
            {
                foreach (var unification in entry.Value.Unifications.ToArray())
                {
                    switch (unification.Expression, unification.Polarity)
                    {
                        case (IPlaceholderTerm placeholder, _):
                            // Alias declared placeholder:
                            if (this.aliases.TryGetValue(placeholder, out var target1))
                            {
                                // Resolved.
                                unification.UpdateExpression(target1);
                            }
                            break;

                        case (_, UnificationPolarities.Both):
                            // Switch an unification to new non-placeholder alias.
                            entry.Value.Unifications.Remove(unification);
                            // Alias declared placeholder:
                            if (this.aliases.TryGetValue(entry.Key, out var target2))
                            {
                                // Narrow and replace
                                var combined = AndExpression.Create(
                                    target2, unification.Expression);
                                var calculated = calculator.Compute(combined);
                                this.aliases[entry.Key] = calculated;
                            }
                            else
                            {
                                // Store
                                this.aliases.Add(entry.Key, unification.Expression);
                            }
                            break;
                        
                        case (_, UnificationPolarities.In):
                            // Alias declared placeholder:
                            if (this.aliases.TryGetValue(entry.Key, out var target3))
                            {
                                // Narrow and check
                                var combined = AndExpression.Create(
                                    target3, unification.Expression);
                                var calculated = calculator.Compute(combined);
                                
                                // Absorb?
                                if (calculator.Equals(calculated, unification.Expression))
                                {
                                    // Switch an unification to new non-placeholder alias.
                                    entry.Value.Unifications.Remove(unification);
                                    this.aliases[entry.Key] = calculated;
                                }
                            }
                            break;

                        case (_, UnificationPolarities.Out):
                            // Alias declared placeholder:
                            if (this.aliases.TryGetValue(entry.Key, out var target4))
                            {
                                // Narrow and check
                                var combined = AndExpression.Create(
                                    target4, unification.Expression);
                                var calculated = calculator.Compute(combined);
                                
                                // Absorb?
                                if (calculator.Equals(calculated, unification.Expression))
                                {
                                    // Switch an unification to new non-placeholder alias.
                                    entry.Value.Unifications.Remove(unification);
                                    this.aliases[entry.Key] = calculated;
                                }
                            }
                            break;
                    }
                }
            }
        }

        #region Resolve
        [DebuggerStepThrough]
        private sealed class ResolveContext
        {
            private readonly ITypeCalculator calculator;
            private readonly Func<IExpression, IExpression, IExpression> creator;

            public readonly UnificationPolarities Polarity;

            private ResolveContext(
                ITypeCalculator calculator,
                UnificationPolarities polarity, 
                Func<IExpression, IExpression, IExpression> creator)
            {
                Debug.Assert(polarity != UnificationPolarities.Both);
                
                this.calculator = calculator;
                this.Polarity = polarity;
                this.creator = creator;
            }

            public IExpression? Compute(IExpression[] expressions) =>
                LogicalCalculator.ConstructNested(expressions, this.creator) is IExpression combined ?
                    this.calculator.Compute(combined) : null;
            
            public static ResolveContext Create(
                ITypeCalculator calculator,
                UnificationPolarities polarity, 
                Func<IExpression, IExpression, IExpression> creator) =>
                new ResolveContext(calculator, polarity, creator);
        }
        
        private IExpression InternalResolve(
            ResolveContext context,
            IPlaceholderTerm placeholder)
        {
            var resolved = this.GetAlias<IExpression>(placeholder, placeholder)!;
            if (resolved is IPlaceholderTerm ph)
            {
                if (this.topology.TryGetValue(ph, out var node))
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
                                    ResolveRecursive(parent.Right));
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
                        var calculated = context.Compute(expressions)!;
                        return calculated;
                    }
                }
            }

            return resolved;
        }
        
        public IExpression Resolve(ITypeCalculator calculator, IPlaceholderTerm placeholder)
        {
            // TODO: cache
            
            var outMost0 = this.InternalResolve(
                ResolveContext.Create(
                    calculator,
                    UnificationPolarities.Out,
                    OrExpression.Create),
                placeholder);
            var inMost0 = this.InternalResolve(
                ResolveContext.Create(
                    calculator,
                    UnificationPolarities.In,
                    AndExpression.Create),
                placeholder);

            switch (outMost0, inMost0)
            {
                case (IPlaceholderTerm _, IPlaceholderTerm imph0):
                    // inmost (narrow) has higher priority.
                    var inMost1 = this.InternalResolve(
                        ResolveContext.Create(
                            calculator,
                            UnificationPolarities.In,
                            AndExpression.Create),
                        imph0);
                    return inMost1;
                case (IPlaceholderTerm _, _):
                    return inMost0;
                case (_, IPlaceholderTerm _):
                    return outMost0;
                default:
                    // Combine both expressions.
                    return calculator.Compute(
                        AndExpression.Create(outMost0, inMost0));
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

        [DebuggerStepThrough]
        public void SetTargetRoot(IExpression targetRoot) =>
#if DEBUG
            this.targetRoot = targetRoot;
#else
            this.targetRootString =
                targetRoot.GetPrettyString(PrettyStringTypes.ReadableAll);
#endif

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
                        var resolved = this.GetAlias<IExpression>(entry.Key, entry.Key)!;
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
            "Topology: " + this.View;

        [DebuggerStepThrough]
        public static Topology Create(IExpression targetRoot) =>
            new Topology(targetRoot);
    }
}
