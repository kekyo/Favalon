using Favalet.Contexts;
using Favalet.Internal;
using System.Collections;
using System.Diagnostics;
using System.Xml.Linq;

namespace Favalet.Expressions
{
    public interface IConstantTerm : ITerm
    {
        object Value { get; }
    }

    public sealed class ConstantTerm :
        Expression, IConstantTerm
    {
        private readonly LazySlim<IExpression> higherOrder;

        public readonly object Value;

        [DebuggerStepThrough]
        private ConstantTerm(object value, LazySlim<IExpression> higherOrder)
        {
            this.Value = value;
            this.higherOrder = higherOrder;
        }

        public override IExpression HigherOrder
        {
            [DebuggerStepThrough]
            get => higherOrder.Value;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object IConstantTerm.Value
        {
            [DebuggerStepThrough]
            get => this.Value;
        }

        public override int GetHashCode() =>
            this.Value.GetHashCode();

        public bool Equals(IConstantTerm rhs) =>
            this.Value.Equals(rhs.Value);

        public override bool Equals(IExpression? other) =>
            other is IConstantTerm rhs && this.Equals(rhs);

        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            new ConstantTerm(
                this.Value,
                LazySlim.Create(context.MakeRewritable(this.HigherOrder)));

        protected override IExpression Infer(IInferContext context) =>
            this;

        protected override IExpression Fixup(IFixupContext context) =>
            this;

        protected override IExpression Reduce(IReduceContext context) =>
            this;

        private string StringValue
        {
            [DebuggerStepThrough]
            get => this.Value switch
            {
                string value => $"\"{value}\"",
                _ => this.Value.ToString()
            };
        }

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            new[] { new XAttribute("value", this.StringValue) };

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                this.StringValue);

        [DebuggerStepThrough]
        public static ConstantTerm From(object value) =>
            new ConstantTerm(value, LazySlim.Create(() =>
                (IExpression)TypeTerm.From(value.GetType())));
    }
}
