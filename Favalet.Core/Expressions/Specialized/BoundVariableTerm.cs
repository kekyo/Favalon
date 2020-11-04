using Favalet.Contexts;
using System.Collections;
using System.Diagnostics;
using System.Xml.Linq;

namespace Favalet.Expressions.Specialized
{
    public interface IBoundVariableTerm :
        ITerm
    {
        string Symbol { get; }
    }

    public sealed class BoundVariableTerm :
        Expression, IBoundVariableTerm
    {
        public readonly string Symbol;

        [DebuggerStepThrough]
        private BoundVariableTerm(string symbol, IExpression higherOrder)
        {
            this.HigherOrder = higherOrder;
            this.Symbol = symbol;
        }

        public override IExpression HigherOrder { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string IBoundVariableTerm.Symbol
        {
            [DebuggerStepThrough]
            get => this.Symbol;
        }

        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            new BoundVariableTerm(
                this.Symbol,
                context.MakeRewritableHigherOrder(this.HigherOrder));

        protected override IExpression Infer(IInferContext context)
        {
            var higherOrder = context.Infer(this.HigherOrder);

            if (object.ReferenceEquals(this.HigherOrder, higherOrder))
            {
                return this;
            }
            else
            {
                return new BoundVariableTerm(this.Symbol, higherOrder);
            }
        }

        protected override IExpression Fixup(IFixupContext context)
        {
            var higherOrder = context.FixupHigherOrder(this.HigherOrder);

            if (object.ReferenceEquals(this.HigherOrder, higherOrder))
            {
                return this;
            }
            else
            {
                return new BoundVariableTerm(this.Symbol, higherOrder);
            }
        }

        protected override IExpression Reduce(IReduceContext context) =>
            this;
        
        public override int GetHashCode() =>
            this.Symbol.GetHashCode();

        public bool Equals(IBoundVariableTerm rhs) =>
            this.Symbol.Equals(rhs.Symbol);

        public override bool Equals(IExpression? other) =>
            other is IBoundVariableTerm rhs && this.Equals(rhs);

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            new[] { new XAttribute("symbol", this.Symbol) };

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                this.Symbol);

        [DebuggerStepThrough]
        public static BoundVariableTerm Create(string symbol, IExpression higherOrder) =>
            new BoundVariableTerm(symbol, higherOrder);
        [DebuggerStepThrough]
        public static BoundVariableTerm Create(string symbol) =>
            new BoundVariableTerm(symbol, UnspecifiedTerm.Instance);
    }
}
