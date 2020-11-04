using System;
using Favalet.Contexts;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Favalet.Expressions.Specialized;

namespace Favalet.Expressions.Algebraic
{
    public interface IBinaryExpression : IExpression
    {
        IExpression Left { get; }
        IExpression Right { get; }
    }

    public abstract class BinaryExpression<TBinaryExpression> :
        Expression, IBinaryExpression, IPairExpression
        where TBinaryExpression : IBinaryExpression
    {
        public readonly IExpression Left;
        public readonly IExpression Right;

        [DebuggerStepThrough]
        protected BinaryExpression(
            IExpression left, IExpression right, IExpression higherOrder)
        {
            this.HigherOrder = higherOrder;
            this.Left = left;
            this.Right = right;
        }

        public override IExpression HigherOrder { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IExpression IBinaryExpression.Left
        {
            [DebuggerStepThrough]
            get => this.Left;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IExpression IBinaryExpression.Right
        {
            [DebuggerStepThrough]
            get => this.Right;
        }
        
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IExpression IPairExpression.Left
        {
            [DebuggerStepThrough]
            get => this.Left;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IExpression IPairExpression.Right
        {
            [DebuggerStepThrough]
            get => this.Right;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Type IPairExpression.IdentityType
        {
            [DebuggerStepThrough]
            get => typeof(TBinaryExpression);
        }

        [DebuggerStepThrough]
        IExpression IPairExpression.Create(IExpression left, IExpression right) =>
            this.OnCreate(left, right, UnspecifiedTerm.Instance);
        
        internal abstract IExpression OnCreate(
            IExpression left, IExpression right, IExpression higherOrder);

        protected sealed override IExpression MakeRewritable(IMakeRewritableContext context) =>
            this.OnCreate(
                context.MakeRewritable(this.Left),
                context.MakeRewritable(this.Right),
                context.MakeRewritableHigherOrder(this.HigherOrder));

        protected sealed override IExpression Infer(IInferContext context)
        {
            var left = context.Infer(this.Left);
            var right = context.Infer(this.Right);
            var higherOrder = context.Infer(this.HigherOrder);

            context.Unify(left.HigherOrder, right.HigherOrder, true);
            
            context.Unify(left.HigherOrder, higherOrder, false);
            context.Unify(right.HigherOrder, higherOrder, false);

            if (object.ReferenceEquals(this.Left, left) &&
                object.ReferenceEquals(this.Right, right) &&
                object.ReferenceEquals(this.HigherOrder, higherOrder))
            {
                return this;
            }
            else
            {
                return this.OnCreate(left, right, higherOrder);
            }
        }

        protected sealed override IExpression Fixup(IFixupContext context)
        {
            var left = context.Fixup(this.Left);
            var right = context.Fixup(this.Right);
            var higherOrder = context.FixupHigherOrder(this.HigherOrder);

            if (object.ReferenceEquals(this.Left, left) &&
                object.ReferenceEquals(this.Right, right) &&
                object.ReferenceEquals(this.HigherOrder, higherOrder))
            {
                return this;
            }
            else
            {
                return this.OnCreate(left, right, higherOrder);
            }
        }

        protected sealed override IExpression Reduce(IReduceContext context)
        {
            var left = context.Reduce(this.Left);
            var right = context.Reduce(this.Right);

            if (object.ReferenceEquals(this.Left, left) &&
                object.ReferenceEquals(this.Right, right))
            {
                return this;
            }
            else
            {
                return this.OnCreate(left, right, this.HigherOrder);
            }
        }

        public override int GetHashCode() =>
            this.Left.GetHashCode() ^ this.Right.GetHashCode();

        public bool Equals(TBinaryExpression rhs) =>
            this.Left.Equals(rhs.Left) && this.Right.Equals(rhs.Right);

        public override bool Equals(IExpression? other) =>
            other is TBinaryExpression rhs && this.Equals(rhs);

        protected sealed override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            new[] { context.GetXml(this.Left), context.GetXml(this.Right) };
    }

    public static class BinaryExpressionExtension
    {
        [DebuggerStepThrough]
        public static void Deconstruct(
            this IBinaryExpression binary,
            out IExpression left,
            out IExpression right)
        {
            left = binary.Left;
            right = binary.Right;
        }
    }
}
