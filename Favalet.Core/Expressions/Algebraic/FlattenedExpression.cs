using Favalet.Contexts;
using Favalet.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Favalet.Expressions.Specialized;

namespace Favalet.Expressions.Algebraic
{
    internal abstract class FlattenedExpression :
        Expression
    {
        public readonly IExpression[] Operands;

        [DebuggerStepThrough]
        protected FlattenedExpression(IExpression[] operands) =>
            this.Operands = operands;

        public sealed override int GetHashCode() =>
            this.Operands.Aggregate(0, (agg, operand) => agg ^ operand?.GetHashCode() ?? 0);

        protected sealed override IExpression MakeRewritable(IMakeRewritableContext context) =>
            throw new InvalidOperationException();

        protected sealed override IExpression Infer(IInferContext context) =>
            throw new InvalidOperationException();

        protected sealed override IExpression Fixup(IFixupContext context) =>
            throw new InvalidOperationException();

        protected sealed override IExpression Reduce(IReduceContext context) =>
            throw new InvalidOperationException();

        protected sealed override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            this.Operands.Select(context.GetXml);

        [DebuggerStepThrough]
        public void Deconstruct(out IExpression[] operands) =>
            operands = this.Operands;

        public static IEnumerable<IExpression> Flatten<TBinaryExpression>(
            IExpression left, IExpression right)
            where TBinaryExpression : IBinaryExpression
        {
            var lf = left is TBinaryExpression lb ?
                Flatten<TBinaryExpression>(lb.Left, lb.Right) :
                new[] { left };

            var rf = right is TBinaryExpression rb ?
                Flatten<TBinaryExpression>(rb.Left, rb.Right) :
                new[] { right };

            return lf.Concat(rf);
        }

        public static IExpression Flatten(IExpression expression) =>
            expression switch
            {
                IAndExpression and => new AndFlattenedExpression(
                    Flatten<IAndExpression>(and.Left, and.Right).Memoize()),
                IOrExpression or => new OrFlattenedExpression(
                    Flatten<IOrExpression>(or.Left, or.Right).Memoize()),
                _ => expression
            };

        public static IEnumerable<IExpression> FlattenAll<TBinaryExpression>(
            IExpression left, IExpression right)
            where TBinaryExpression : IBinaryExpression
        {
            var lf = left is TBinaryExpression lb ?
                FlattenAll<TBinaryExpression>(lb.Left, lb.Right) :
                new[] { FlattenAll(left) };

            var rf = right is TBinaryExpression rb ?
                FlattenAll<TBinaryExpression>(rb.Left, rb.Right) :
                new[] { FlattenAll(right) };

            return lf.Concat(rf);
        }

        public static IExpression FlattenAll(IExpression expression) =>
            expression switch
            {
                IAndExpression and => new AndFlattenedExpression(
                    FlattenAll<IAndExpression>(and.Left, and.Right).Memoize()),
                IOrExpression or => new OrFlattenedExpression(
                    FlattenAll<IOrExpression>(or.Left, or.Right).Memoize()),
                _ => expression
            };
    }

    internal sealed class AndFlattenedExpression : FlattenedExpression
    {
        [DebuggerStepThrough]
        public AndFlattenedExpression(IExpression[] operands) :
            base(operands)
        { }

        public override IExpression HigherOrder =>
            new AndFlattenedExpression(
                this.Operands.Select(operand => operand.HigherOrder).Memoize());

        public override bool Equals(IExpression? other) =>
            other is AndFlattenedExpression rhs &&
            this.Operands.EqualsPartiallyOrdered(rhs.Operands);

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                StringUtilities.Join(
                    " && ",
                    this.Operands.Select(context.GetPrettyString)));
    }

    internal sealed class OrFlattenedExpression : FlattenedExpression
    {
        [DebuggerStepThrough]
        public OrFlattenedExpression(IExpression[] operands) :
            base(operands)
        { }

        public override IExpression HigherOrder =>
            new OrFlattenedExpression(
                this.Operands.Select(operand => operand.HigherOrder).Memoize());

        public override bool Equals(IExpression? other) =>
            other is OrFlattenedExpression rhs &&
            this.Operands.EqualsPartiallyOrdered(rhs.Operands);

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                StringUtilities.Join(
                    " || ",
                    this.Operands.Select(context.GetPrettyString)));
    }
}
