using Favalet.Contexts;
using System.Collections;
using System.Diagnostics;
using System.Linq;

namespace Favalet.Expressions.Specialized
{
    [DebuggerStepThrough]
    internal sealed class DeadEndTerm :
        Expression, IIgnoreUnificationTerm
    {
        private DeadEndTerm()
        { }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public override IExpression HigherOrder =>
            Instance;

        public override bool Equals(IExpression? other) =>
            false;

        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            this;

        protected override IExpression Infer(IInferContext context) =>
            this;

        protected override IExpression Fixup(IFixupContext context) =>
            this;

        protected override IExpression Reduce(IReduceContext context) =>
            this;

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            Enumerable.Empty<object>();

        protected override string GetPrettyString(IPrettyStringContext context) =>
            "#DE";

        public static readonly DeadEndTerm Instance =
            new DeadEndTerm();
    }

    [DebuggerStepThrough]
    public static class DeadEndTermExtension
    {
        public static bool IsDeadEnd(this IExpression expression) =>
            expression is DeadEndTerm;
    }
}
