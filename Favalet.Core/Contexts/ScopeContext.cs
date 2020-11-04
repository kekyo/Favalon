using Favalet.Expressions;
using Favalet.Expressions.Specialized;
using Favalet.Internal;
using System.Collections.Generic;
using System.Diagnostics;

namespace Favalet.Contexts
{
    public readonly struct VariableInformation
    {
#if DEBUG
        public readonly string Symbol;
#endif
        public readonly IExpression SymbolHigherOrder;
        public readonly IExpression Expression;

        private VariableInformation(
            string symbol, IExpression symbolHigherOrder, IExpression expression)
        {
#if DEBUG
            this.Symbol = symbol;
#endif
            this.SymbolHigherOrder = symbolHigherOrder;
            this.Expression = expression;
        }

        public override string ToString() =>
#if DEBUG
            $"{this.Symbol}:{this.SymbolHigherOrder.GetPrettyString(PrettyStringTypes.Readable)} --> {this.Expression.GetPrettyString(PrettyStringTypes.Readable)}";
#else
            $"{this.SymbolHigherOrder.GetPrettyString(PrettyStringTypes.Readable)} --> {this.Expression.GetPrettyString(PrettyStringTypes.Readable)}";
#endif
        public static VariableInformation Create(
            string symbol, IExpression symbolHigherOrder, IExpression expression) =>
            new VariableInformation(symbol, symbolHigherOrder, expression);
    }

    public interface IScopeContext
    {
        ITypeCalculator TypeCalculator { get; }

        VariableInformation[] LookupVariables(string symbol);
    }

    public abstract class ScopeContext :
        IScopeContext
    {
        private readonly ScopeContext? parent;
        private Dictionary<string, List<VariableInformation>>? variables;

        [DebuggerStepThrough]
        internal ScopeContext(ScopeContext? parent, ITypeCalculator typeCalculator)
        {
            this.parent = parent;
            this.TypeCalculator = typeCalculator;
        }

        public ITypeCalculator TypeCalculator { get; }

        private protected void MutableBind(IBoundVariableTerm symbol, IExpression expression)
        {
            this.variables ??= new Dictionary<string, List<VariableInformation>>();

            if (!this.variables.TryGetValue(symbol.Symbol, out var list))
            {
                list = new List<VariableInformation>();
                this.variables.Add(symbol.Symbol, list);
            }

            list.Add(
                VariableInformation.Create(
                    symbol.Symbol,
                    symbol.HigherOrder,
                    expression));
        }

        public VariableInformation[] LookupVariables(string symbol)
        {
            if (this.variables != null &&
                this.variables.TryGetValue(symbol, out var list))
            {
                return list.Memoize();
            }
            else
            {
                return
                    this.parent?.LookupVariables(symbol) ??
                    ArrayEx.Empty<VariableInformation>();
            }
        }
    }
}
