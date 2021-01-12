/////////////////////////////////////////////////////////////////////////////////////////////////
//
// Favalon - An Interactive Shell Based on a Typed Lambda Calculus.
// Copyright (c) 2018-2020 Kouji Matsui (@kozy_kekyo, @kekyo2)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
/////////////////////////////////////////////////////////////////////////////////////////////////

using Favalet.Contexts;
using Favalet.Expressions.Specialized;
using Favalet.Internal;
using Favalet.Ranges;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Favalet.Expressions
{
    public interface ICallableExpression : IExpression
    {
        IExpression Call(IReduceContext context, IExpression reducedArgument);
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
            IExpression higherOrder,
            TextRange range) :
            base(range)
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
        IExpression IPairExpression.Create(
            IExpression left, IExpression right, TextRange range) =>
            Create(left, right, range);

        public override int GetHashCode() =>
            this.Function.GetHashCode() ^ this.Argument.GetHashCode();

        public bool Equals(IApplyExpression rhs) =>
            this.Function.Equals(rhs.Function) &&
            this.Argument.Equals(rhs.Argument);

        public override bool Equals(IExpression? other) =>
            other is IApplyExpression rhs && this.Equals(rhs);

        private IEnumerable<IExpression> EnumerateRecursively(ITransposeContext context)
        {
            var stack = new Stack<IExpression>();
            stack.Push(this.Argument);
            stack.Push(this.Function);
            
            while (stack.Count >= 1)
            {
                var current = stack.Pop();
                if (current is IApplyExpression apply)
                {
                    stack.Push(apply.Argument);
                    stack.Push(apply.Function);
                    continue;
                }

                var transposed = context.Transpose(current);
                yield return transposed;
            }
        }

        private static IExpression TransposeCore(IEnumerable<IExpression> expressions)
        {
            var arguments = new LinkedList<IExpression>();
            var stack = new LinkedList<IExpression>();

            foreach (var expr in expressions)
            {
                switch (expr)
                {
                    case IVariableTerm(_, ({} position, {} associativity, {} precedence)):
                        IExpression? current = expr;
                        while (current != null)
                        {
                            
                        }
                        break;

                    default:
                        arguments.AddFirst(expr);
                        break;
                }
            }
        }

        protected override IExpression Transpose(ITransposeContext context)
        {
#if DEBUG
            var seq = this.EnumerateRecursively(context).Memoize();
            var result = TransposeCore(seq);
            return result;
#else
            return TransposeCore(this.EnumerateRecursively(context));
#endif
        }

        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            new ApplyExpression(
                context.MakeRewritable(this.Function),
                context.MakeRewritable(this.Argument),
                context.MakeRewritableHigherOrder(this.HigherOrder),
                this.Range);

        protected override IExpression Infer(IInferContext context)
        {
            var argument = context.Infer(this.Argument);
            var function = context.Infer(this.Function);
            var higherOrder = context.Infer(this.HigherOrder);

            var functionHigherOrder = LambdaExpression.Create(
                argument.HigherOrder, higherOrder, this.Range);  // TODO: range

            context.Unify(function.HigherOrder, functionHigherOrder, false);

            if (object.ReferenceEquals(this.Argument, argument) &&
                object.ReferenceEquals(this.Function, function) &&
                object.ReferenceEquals(this.HigherOrder, higherOrder))
            {
                return this;
            }
            else
            {
                return new ApplyExpression(function, argument, higherOrder, this.Range);
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
                return new ApplyExpression(function, argument, higherOrder, this.Range);
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
                    var result = lambda.Invoke(context, this.Argument);
                    return context.Reduce(result);
                }

                // Apply with right outermost strategy,
                // because maybe cannot analyze inside of the function.
                if (currentFunction is ICallableExpression callable)
                {
                    var reducedArgument = context.Reduce(this.Argument);
                    return callable.Call(context, reducedArgument);
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
                            this.HigherOrder,
                            this.Range);
                    }
                }
                
                if (object.ReferenceEquals(currentFunction, reducedFunction))
                {
                    var argument = context.Reduce(this.Argument);
                    return new ApplyExpression(
                        reducedFunction,
                        argument,
                        this.HigherOrder,
                        this.Range);
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
            IExpression function, IExpression argument, TextRange range) =>
            new ApplyExpression(function, argument, UnspecifiedTerm.Instance, range);

        [DebuggerStepThrough]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ApplyExpression UnsafeCreate(
            IExpression function, IExpression argument, IExpression higherOrder, TextRange range) =>
            // Will drop higher order expression when inferring.
            new ApplyExpression(function, argument, higherOrder, range);
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
