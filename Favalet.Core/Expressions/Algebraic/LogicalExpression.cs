using Favalet.Contexts;
using Favalet.Expressions.Specialized;
using System.Collections;
using System.Diagnostics;
using Favalet.Internal;

namespace Favalet.Expressions.Algebraic
{
    public interface ILogicalExpression : IExpression
    {
        IExpression Operand { get; }
    }

    public sealed class LogicalExpression :
        Expression, ILogicalExpression
    {
        public readonly IExpression Operand;

        [DebuggerStepThrough]
        private LogicalExpression(
            IExpression operand, IExpression higherOrder)
        {
            this.HigherOrder = higherOrder;
            this.Operand = operand;
        }

        public override IExpression HigherOrder { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IExpression ILogicalExpression.Operand
        {
            [DebuggerStepThrough]
            get => this.Operand;
        }

        public override int GetHashCode() =>
            this.Operand.GetHashCode();

        public bool Equals(ILogicalExpression rhs) =>
            LogicalCalculator.Instance.Equals(this.Operand, rhs.Operand);

        public override bool Equals(IExpression? other) =>
            other is ILogicalExpression rhs && this.Equals(rhs);
        
        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            new LogicalExpression(
                context.MakeRewritable(this.Operand),
                context.MakeRewritable(this.HigherOrder));

        protected override IExpression Infer(IInferContext context)
        {
            var higherOrder = context.Infer(this.HigherOrder);
            var operand = context.Infer(this.Operand);

            context.Unify(operand.HigherOrder, higherOrder, false);

            if (object.ReferenceEquals(this.HigherOrder, higherOrder) &&
                object.ReferenceEquals(this.Operand, operand))
            {
                return this;
            }
            else
            {
                return new LogicalExpression(operand, higherOrder);
            }
        }

        protected override IExpression Fixup(IFixupContext context)
        {
            var higherOrder = context.Fixup(this.HigherOrder);
            var operand = context.Fixup(this.Operand);

            if (object.ReferenceEquals(this.HigherOrder, higherOrder) &&
                object.ReferenceEquals(this.Operand, operand))
            {
                return this;
            }
            else
            {
                return new LogicalExpression(operand, higherOrder);
            }
        }

        protected override IExpression Reduce(IReduceContext context) =>
            LogicalCalculator.Instance.Compute(context.Reduce(this.Operand));

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            new[] { context.GetXml(this.Operand) };

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                context.GetPrettyString(this.Operand));

        [DebuggerStepThrough]
        public static LogicalExpression Create(
            IExpression operand, IExpression higherOrder) =>
            new LogicalExpression(operand, higherOrder);
        [DebuggerStepThrough]
        public static LogicalExpression Create(
            IExpression operand) =>
            new LogicalExpression(operand, UnspecifiedTerm.Instance);
    }
}
