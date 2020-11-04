using Favalet.Contexts;
using System.Collections;
using System.Diagnostics;
using System.Linq;

namespace Favalet.Expressions.Specialized
{
    [DebuggerStepThrough]
    public sealed class FourthTerm :
        Expression, ITerm
    {
        private FourthTerm()
        { }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public override IExpression HigherOrder =>
            DeadEndTerm.Instance;

        public bool Equals(FourthTerm rhs) =>
            rhs != null;

        public override bool Equals(IExpression? other) =>
            other is FourthTerm;

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
            "#";

        public static readonly FourthTerm Instance =
            new FourthTerm();
    }
}
