using Favalet.Contexts;
using Favalet.Expressions.Specialized;
using System.Diagnostics;

namespace Favalet.Expressions.Algebraic
{
    public interface IOrExpression : IBinaryExpression
    {
    }

    public sealed class OrExpression :
        BinaryExpression<IOrExpression>,
        IOrExpression
    {
        [DebuggerStepThrough]
        private OrExpression(
            IExpression left, IExpression right, IExpression higherOrder) :
            base(left, right, higherOrder)
        { }

        [DebuggerStepThrough]
        internal override IExpression OnCreate(
            IExpression left, IExpression right, IExpression higherOrder) =>
            new OrExpression(left, right, higherOrder);

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                $"{context.GetPrettyString(this.Left)} || {context.GetPrettyString(this.Right)}");

        [DebuggerStepThrough]
        public static OrExpression Create(
            IExpression left, IExpression right, IExpression higherOrder) =>
            new OrExpression(left, right, higherOrder);
        [DebuggerStepThrough]
        public static OrExpression Create(
            IExpression left, IExpression right) =>
            new OrExpression(left, right, UnspecifiedTerm.Instance);
    }
}
