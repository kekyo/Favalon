using Favalet.Contexts;
using Favalet.Expressions.Specialized;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using Favalet.Internal;

namespace Favalet.Expressions.Algebraic
{
    public interface ILogicalOperator :
        ICallableExpression
    {
    }

    public sealed class LogicalOperator :
        Expression, ILogicalOperator
    {
        private static readonly IExpression higherOrder =
            FunctionExpression.Create(UnspecifiedTerm.Instance, UnspecifiedTerm.Instance);

        private LogicalOperator()
        { }

        public override IExpression HigherOrder =>
            higherOrder;

        public bool Equals(ILogicalOperator rhs) =>
            rhs != null;

        public override bool Equals(IExpression? other) =>
            other is ILogicalOperator rhs && this.Equals(rhs);
        
        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            this;

        protected override IExpression Infer(IInferContext context) =>
            this;

        protected override IExpression Fixup(IFixupContext context) =>
            this;

        protected override IExpression Reduce(IReduceContext context) =>
            this;

        public IExpression Call(IReduceContext context, IExpression argument) =>
            LogicalCalculator.Instance.Compute(argument);

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            Enumerable.Empty<object>();

        protected override string GetPrettyString(IPrettyStringContext context) =>
            "Logical";

        public static readonly LogicalOperator Instance =
            new LogicalOperator();
    }
}
