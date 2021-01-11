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
            var enumerator = expressions.GetEnumerator();
            try
            {
                var f = enumerator.MoveNext();
                Debug.Assert(f);

                var stack = new Stack<(BoundPrecedences p, IExpression ex)>();
                var queue = new LinkedList<IExpression>();

                var next = enumerator.Current!;
                queue.AddLast(next);
                var lastPrecedence = next is IVariableTerm(_, (_, _, { } p0)) ?
                    p0 : BoundPrecedences.Neutral;

                while (enumerator.MoveNext())
                {
                    next = enumerator.Current!;

                    switch (next)
                    {
                        case IVariableTerm(_, (BoundPositions.Infix, { } a, { } p)):
                            if (p <= lastPrecedence)
                            {
                                if (stack.Count >= 1 &&
                                    stack.Peek() is ({ } sp, { } se) &&
                                    p <= sp)
                                {
                                    stack.Pop();
                                    var folded = queue.Aggregate((l, r) => Create(l, r, l.Range.Combine(r.Range)));
                                    var combined = Create(se, folded, se.Range.Combine(folded.Range));
                                    queue.Clear();
                                    queue.AddLast(next);
                                    queue.AddLast(combined);
                                }
                                else
                                {
                                    if (queue.Count >= 2)
                                    {
                                        var folded = queue.Aggregate((l, r) => Create(l, r, l.Range.Combine(r.Range)));
                                        queue.Clear();
                                        queue.AddLast(next);
                                        queue.AddLast(folded);
                                    }
                                    else
                                    {
                                        queue.AddFirst(next);
                                    }
                                }
                            }
                            else
                            {
                                if (queue.Count >= 2)
                                {
                                    var last = queue.Last!;
                                    queue.RemoveLast();
                                    var folded = queue.Aggregate((l, r) => Create(l, r, l.Range.Combine(r.Range)));
                                    stack.Push((lastPrecedence, folded));
                                    queue.Clear();
                                    queue.AddLast(next);
                                    queue.AddLast(last);
                                }
                                else
                                {
                                    queue.AddFirst(next);
                                }
                            }
                            lastPrecedence = p;
                            break;

                        default:
                            queue.AddLast(next);
                            break;
                    }
                }

                var final = queue.Aggregate((l, r) => Create(l, r, l.Range.Combine(r.Range)));
                while (stack.Count >= 1)
                {
                    var (_, left) = stack.Pop();
                    final = Create(left, final, left.Range.Combine(final.Range));
                }
                
                return final;
            }
            finally
            {
                if (enumerator is IDisposable d)
                {
                    d.Dispose();
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
