using Favalet.Contexts;
using Favalet.Expressions.Specialized;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Favalet.Expressions.Algebraic;
using Favalet.Internal;

namespace Favalet.Expressions
{
    public interface IVariableTerm :
        IIdentityTerm
    {
    }
    
    public sealed class VariableTerm :
        Expression, IVariableTerm
    {
        private readonly IExpression[]? bounds;
        
        public readonly string Symbol;

        [DebuggerStepThrough]
        private VariableTerm(string symbol, IExpression higherOrder, IExpression[]? bounds)
        {
            this.HigherOrder = higherOrder;
            this.Symbol = symbol;
            this.bounds = bounds;
        }

        public override IExpression HigherOrder { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string IIdentityTerm.Symbol
        {
            [DebuggerStepThrough]
            get => this.Symbol;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object IIdentityTerm.Identity
        {
            [DebuggerStepThrough]
            get => this.Symbol;
        }

        public override int GetHashCode() =>
            this.Symbol.GetHashCode();

        public bool Equals(IIdentityTerm rhs) =>
            this.Symbol.Equals(rhs.Symbol);

        public override bool Equals(IExpression? other) =>
            other is IIdentityTerm rhs && this.Equals(rhs);

        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            new VariableTerm(
                this.Symbol,
                context.MakeRewritableHigherOrder(this.HigherOrder),
                this.bounds);
        
        protected override IExpression Infer(IInferContext context)
        {
            if (this.bounds is IExpression[])
            {
                return this;
            }
            
            var higherOrder = context.Infer(this.HigherOrder);
            var variables = context.LookupVariables(this.Symbol);

            if (variables.Length >= 1)
            {
                var targets = variables.
                    Where(v => !context.TypeCalculator.ExactEquals(this, v.Expression)).
                    Select(v =>
                        (symbolHigherOrder: context.Infer(context.MakeRewritableHigherOrder(v.SymbolHigherOrder)), 
                         expression: context.Infer(context.MakeRewritable(v.Expression)))).
                    Memoize();

                if (targets.Length >= 1)
                {
                    var symbolHigherOrder = LogicalCalculator.ConstructNested(
                        targets.Select(v => v.symbolHigherOrder).Memoize(),
                        OrExpression.Create)!;

                    var expressionHigherOrder = LogicalCalculator.ConstructNested(
                        targets.Select(v => v.expression.HigherOrder).Memoize(),
                        OrExpression.Create)!;
               
                    context.Unify(symbolHigherOrder, expressionHigherOrder, true);
                    context.Unify(expressionHigherOrder, higherOrder, true);
                }
                
                var bounds = targets.
                    Select(entry => entry.expression).
                    Memoize();
                return new VariableTerm(this.Symbol, higherOrder, bounds);
            }

            if (object.ReferenceEquals(this.HigherOrder, higherOrder))
            {
                return this;
            }
            else
            {
                return new VariableTerm(this.Symbol, higherOrder, this.bounds);
            }
        }

        protected override IExpression Fixup(IFixupContext context)
        {
            var higherOrder = context.FixupHigherOrder(this.HigherOrder);

            if (this.bounds is IExpression[])
            {
                if (this.bounds.Length >= 1)
                {
                    var targets = this.bounds.
                        Select(context.Fixup).
                        Memoize();

                    if (targets.Length >= 1)
                    {
                        var targetsHigherOrder = LogicalCalculator.ConstructNested(
                            targets.
                                Select(target => target.HigherOrder).
                                Memoize(),
                            OrExpression.Create)!;

                        var calculated = context.TypeCalculator.Compute(
                            AndExpression.Create(
                                higherOrder, targetsHigherOrder));

                        var filteredTargets = targets.
                            Select(target =>
                                (target,
                                 calculated: context.TypeCalculator.Compute(
                                    OrExpression.Create(target.HigherOrder, calculated)))).
                            Where(entry => entry.calculated.Equals(calculated)).
                            Select(entry => entry.target).
                            Memoize();

                        if (filteredTargets.Length >= 1)
                        {
                            return new VariableTerm(this.Symbol, higherOrder, filteredTargets);
                        }
                    }
                }
            }

            if (object.ReferenceEquals(this.HigherOrder, higherOrder))
            {
                return this;
            }
            else
            {
                return new VariableTerm(this.Symbol, higherOrder, this.bounds);
            }
        }

        protected override IExpression Reduce(IReduceContext context)
        {
            if (this.bounds is IExpression[] bounds)
            {
                Debug.Assert(bounds.Length >= 1);

                var target = bounds[0];
                if (target is IBoundVariableTerm bound)
                {
                    var variables = context.LookupVariables(bound.Symbol);
                    if (variables.Length >= 1)
                    {
                        // Nearly overloaded variable.
                        return context.Reduce(variables[0].Expression);
                    }
                }
                else
                {
                    return context.Reduce(target);
                }
            }

            return this;
        }

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            new[] { new XAttribute("symbol", this.Symbol) };

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                this.Symbol);

        [DebuggerStepThrough]
        public static VariableTerm Create(string symbol, IExpression higherOrder) =>
            new VariableTerm(symbol, higherOrder, default);
        [DebuggerStepThrough]
        public static VariableTerm Create(string symbol) =>
            new VariableTerm(symbol, UnspecifiedTerm.Instance, default);
    }
}
