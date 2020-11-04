using Favalet.Contexts;
using Favalet.Expressions.Specialized;
using System.Diagnostics;

namespace Favalet.Expressions.Algebraic
{
    public interface IAndExpression : IBinaryExpression
    {
    }

    public sealed class AndExpression :
        BinaryExpression<IAndExpression>,
        IAndExpression
    {
        [DebuggerStepThrough]
        private AndExpression(
            IExpression left, IExpression right, IExpression higherOrder) :
            base(left, right, higherOrder)
        { }

        [DebuggerStepThrough]
        internal override IExpression OnCreate(
            IExpression left, IExpression right, IExpression higherOrder) =>
            new AndExpression(left, right, higherOrder);

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                $"{context.GetPrettyString(this.Left)} && {context.GetPrettyString(this.Right)}");

        [DebuggerStepThrough]
        public static AndExpression Create(
            IExpression left, IExpression right, IExpression higherOrder) =>
            new AndExpression(left, right, higherOrder);
        [DebuggerStepThrough]
        public static AndExpression Create(
            IExpression left, IExpression right) =>
            new AndExpression(left, right, UnspecifiedTerm.Instance);
    }
}
