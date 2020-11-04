using System;
using Favalet.Contexts;
using Favalet.Expressions.Specialized;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Favalet.Expressions
{
    public interface ICallableExpression : IExpression
    {
        IExpression Call(IReduceContext context, IExpression argument);
    }

    public interface IApplyExpression : IExpression
    {
        IExpression Function { get; }

        IExpression Argument { get; }
    }

    public sealed class ApplyExpression :
        Expression, IApplyExpression, IPairExpression
    {
        public readonly IExpression Function;
        public readonly IExpression Argument;

        [DebuggerStepThrough]
        private ApplyExpression(
            IExpression function,
            IExpression argument,
            IExpression higherOrder)
        {
            this.HigherOrder = higherOrder;
            this.Function = function;
            this.Argument = argument;
        }

        public override IExpression HigherOrder { get; }
        
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IExpression IApplyExpression.Function
        {
            [DebuggerStepThrough]
            get => this.Function;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IExpression IApplyExpression.Argument
        {
            [DebuggerStepThrough]
            get => this.Argument;
        }


        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IExpression IPairExpression.Left
        {
            [DebuggerStepThrough]
            get => this.Function;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IExpression IPairExpression.Right
        {
            [DebuggerStepThrough]
            get => this.Argument;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Type IPairExpression.IdentityType
        {
            [DebuggerStepThrough]
            get => typeof(IApplyExpression);
        }

        [DebuggerStepThrough]
        IExpression IPairExpression.Create(IExpression left, IExpression right) =>
            Create(left, right);

        public override int GetHashCode() =>
            this.Function.GetHashCode() ^ this.Argument.GetHashCode();

        public bool Equals(IApplyExpression rhs) =>
            this.Function.Equals(rhs.Function) &&
            this.Argument.Equals(rhs.Argument);

        public override bool Equals(IExpression? other) =>
            other is IApplyExpression rhs && this.Equals(rhs);

        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            new ApplyExpression(
                context.MakeRewritable(this.Function),
                context.MakeRewritable(this.Argument),
                context.MakeRewritableHigherOrder(this.HigherOrder));

        protected override IExpression Infer(IInferContext context)
        {
            var argument = context.Infer(this.Argument);
            var function = context.Infer(this.Function);
            var higherOrder = context.Infer(this.HigherOrder);

            var functionHigherOrder = AppliedFunctionExpression.Create(
                argument.HigherOrder, higherOrder);

            context.Unify(function.HigherOrder, functionHigherOrder, false);

            if (object.ReferenceEquals(this.Argument, argument) &&
                object.ReferenceEquals(this.Function, function) &&
                object.ReferenceEquals(this.HigherOrder, higherOrder))
            {
                return this;
            }
            else
            {
                return new ApplyExpression(function, argument, higherOrder);
            }
        }

        protected override IExpression Fixup(IFixupContext context)
        {
            var argument = context.Fixup(this.Argument);
            var function = context.Fixup(this.Function);
            var higherOrder = context.FixupHigherOrder(this.HigherOrder);

            if (object.ReferenceEquals(this.Argument, argument) &&
                object.ReferenceEquals(this.Function, function) &&
                object.ReferenceEquals(this.HigherOrder, higherOrder))
            {
                return this;
            }
            else
            {
                return new ApplyExpression(function, argument, higherOrder);
            }
        }

        protected override IExpression Reduce(IReduceContext context)
        {
            var currentFunction = this.Function;
            while (true)
            {
                // Apply with left outermost strategy at lambda expression.
                if (currentFunction is ILambdaExpression lambda)
                {
                    var result = lambda.Call(context, this.Argument);
                    return context.Reduce(result);
                }

                // Apply with right outermost strategy,
                // because maybe cannot analyze inside of the function.
                if (currentFunction is ICallableExpression callable)
                {
                    var argument = context.Reduce(this.Argument);
                    return callable.Call(context, argument);
                }

                var reducedFunction = context.Reduce(currentFunction);

                if (object.ReferenceEquals(this.Function, reducedFunction))
                {
                    var argument = context.Reduce(this.Argument);
                    if (object.ReferenceEquals(this.Argument, argument))
                    {
                        return this;
                    }
                    else
                    {
                        return new ApplyExpression(
                            reducedFunction,
                            argument,
                            this.HigherOrder);
                    }
                }
                
                if (object.ReferenceEquals(currentFunction, reducedFunction))
                {
                    var argument = context.Reduce(this.Argument);
                    return new ApplyExpression(
                        reducedFunction,
                        argument,
                        this.HigherOrder);
                }

                currentFunction = reducedFunction;
            }
        }

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            new[] { context.GetXml(this.Function), context.GetXml(this.Argument) };

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                $"{context.GetPrettyString(this.Function)} {context.GetPrettyString(this.Argument)}");

        [DebuggerStepThrough]
        public static ApplyExpression Create(
            IExpression function, IExpression argument, IExpression higherOrder) =>
            new ApplyExpression(function, argument, higherOrder);
        [DebuggerStepThrough]
        public static ApplyExpression Create(
            IExpression function, IExpression argument) =>
            new ApplyExpression(function, argument, UnspecifiedTerm.Instance);
    }

    [DebuggerStepThrough]
    public static class ApplyExpressionExtension
    {
        public static void Deconstruct(
            this IApplyExpression apply,
            out IExpression function,
            out IExpression argument)
        {
            function = apply.Function;
            argument = apply.Argument;
        }
    }
}
