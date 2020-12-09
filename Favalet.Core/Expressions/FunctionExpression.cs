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
using Favalet.Ranges;
using System;
using System.Collections;
using System.Diagnostics;

namespace Favalet.Expressions
{
    public interface IFunctionExpression :
        ILambdaExpression
    {
        new IExpression Parameter { get; }
    }

    public sealed class FunctionExpression :
        Expression, IFunctionExpression, IPairExpression
    {
        #region Factory
        [DebuggerStepThrough]
        private sealed class FunctionExpressionFactory
        {
            private readonly IFunctionExpression fourth;

            private FunctionExpressionFactory() =>
                this.fourth = this.OnCreate(
                    FourthTerm.Instance,
                    FourthTerm.Instance,
                    DeadEndTerm.Instance,
                    TextRange.Unknown);

            private IFunctionExpression OnCreate(
                IExpression parameter, IExpression body, IExpression higherOrder, TextRange range) =>
                new FunctionExpression(parameter, body, higherOrder, range);
            
            public IExpression Create(
                IExpression parameter, IExpression body, Func<IExpression> higherOrder, TextRange range)
            {
                switch (parameter, body)
                {
                    case (DeadEndTerm _, _):
                    case (_, DeadEndTerm _):
                        return DeadEndTerm.Instance;
                    case (FourthTerm _, FourthTerm _):
                        return this.fourth;
                    case (FourthTerm _, _):
                    case (_, FourthTerm _):
                        return this.OnCreate(
                            parameter,
                            body,
                            DeadEndTerm.Instance,
                            range);
                    default:
                        return this.OnCreate(
                            parameter,
                            body,
                            higherOrder(),
                            range);
                };
            }

            public IFunctionExpression Create(
                IExpression parameter, IExpression body, IFunctionExpression higherOrder, TextRange range) =>
                (IFunctionExpression)this.Create(
                    parameter,
                    body,
                    () => higherOrder,
                    range);

            private IExpression CreateRecursive(
                IExpression parameter, IExpression body, TextRange range) =>
                this.Create(
                    parameter,
                    body,
                    () => (parameter, body) switch
                    {
                        (UnspecifiedTerm _, UnspecifiedTerm _) =>
                            DeadEndTerm.Instance,
                        (UnspecifiedTerm _, _) =>
                            this.CreateRecursive(UnspecifiedTerm.Instance, body.HigherOrder, TextRange.Unknown),
                        (_, UnspecifiedTerm _) =>
                            this.CreateRecursive(parameter.HigherOrder, UnspecifiedTerm.Instance, TextRange.Unknown),
                        _ =>
                            this.CreateRecursive(parameter.HigherOrder, body.HigherOrder, TextRange.Unknown)
                    },
                    range);

            public IFunctionExpression Create(
                IExpression parameter, IExpression body, TextRange range) =>
                (IFunctionExpression)this.CreateRecursive(parameter, body, range);

            public static readonly FunctionExpressionFactory Instance =
                new FunctionExpressionFactory();
        }
        #endregion
        
        public readonly IExpression Parameter;
        public readonly IExpression Body;

        [DebuggerStepThrough]
        private FunctionExpression(
            IExpression parameter, IExpression body, IExpression higherOrder, TextRange range) :
            base(range)
        {
            this.Parameter = parameter;
            this.Body = body;
            this.HigherOrder = higherOrder;
        }

        public override IExpression HigherOrder { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IExpression IFunctionExpression.Parameter
        {
            [DebuggerStepThrough]
            get => this.Parameter;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IBoundVariableTerm ILambdaExpression.Parameter
        {
            [DebuggerStepThrough]
            get => (IBoundVariableTerm)this.Parameter;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IExpression ILambdaExpression.Body
        {
            [DebuggerStepThrough]
            get => this.Body;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IExpression IPairExpression.Left
        {
            [DebuggerStepThrough]
            get => this.Parameter;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IExpression IPairExpression.Right
        {
            [DebuggerStepThrough]
            get => this.Body;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Type IPairExpression.IdentityType
        {
            [DebuggerStepThrough]
            get => typeof(IFunctionExpression);
        }

        [DebuggerStepThrough]
        IExpression IPairExpression.Create(
            IExpression left, IExpression right, IExpression higherOrder, TextRange range)
        {
            Debug.Assert(higherOrder is UnspecifiedTerm);

            return FunctionExpressionFactory.Instance.Create(left, right, range);
        }

        public override int GetHashCode() =>
            this.Parameter.GetHashCode() ^ this.Body.GetHashCode();

        public bool Equals(IFunctionExpression rhs) =>
            this.Parameter.Equals(rhs.Parameter) &&
            this.Body.Equals(rhs.Body);

        public override bool Equals(IExpression? other) =>
            other is IFunctionExpression rhs && this.Equals(rhs);

        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            FunctionExpressionFactory.Instance.Create(
                context.MakeRewritable(this.Parameter),
                context.MakeRewritable(this.Body),
                () => context.MakeRewritableHigherOrder(this.HigherOrder),
                this.Range);

        protected override IExpression Infer(IInferContext context)
        {
            var parameter = context.Infer(this.Parameter);

            var scoped = parameter is IBoundVariableTerm bound ?
                context.Bind(bound, bound) :
                context;

            var body = scoped.Infer(this.Body);

            // Recursive inferring exit rule.
            if (parameter is FourthTerm || body is FourthTerm ||
                parameter.HigherOrder is DeadEndTerm || body.HigherOrder is DeadEndTerm)
            {
                if (object.ReferenceEquals(this.Parameter, parameter) &&
                    object.ReferenceEquals(this.Body, body) &&
                    this.HigherOrder is DeadEndTerm)
                {
                    return this;
                }
                else
                {
                    return FunctionExpressionFactory.Instance.Create(
                        parameter,
                        body,
                        () => DeadEndTerm.Instance,
                        this.Range);
                }
            }
            else
            {
                var higherOrder = context.Infer(this.HigherOrder);

                var functionHigherOrder = FunctionExpressionFactory.Instance.Create(
                    parameter.HigherOrder, body.HigherOrder, this.Range);

                context.Unify(functionHigherOrder, higherOrder, false);

                if (object.ReferenceEquals(this.Parameter, parameter) &&
                    object.ReferenceEquals(this.Body, body) &&
                    object.ReferenceEquals(this.HigherOrder, higherOrder))
                {
                    return this;
                }
                else
                {
                    return FunctionExpressionFactory.Instance.Create(
                        parameter,
                        body,
                        () => higherOrder,
                        this.Range);
                }
            }
        }

        protected override IExpression Fixup(IFixupContext context)
        {
            var parameter = context.Fixup(this.Parameter);
            var body = context.Fixup(this.Body);
            var higherOrder = context.FixupHigherOrder(this.HigherOrder);

            if (object.ReferenceEquals(this.Parameter, parameter) &&
                object.ReferenceEquals(this.Body, body) &&
                object.ReferenceEquals(this.HigherOrder, higherOrder))
            {
                return this;
            }
            else if (higherOrder is IFunctionExpression functionExpression)
            {
                return FunctionExpressionFactory.Instance.Create(
                    parameter, body, functionExpression, this.Range);
            }
            else
            {
                // TODO: Apply fixed up higher order.
                //return InternalCreate(parameter, body, () => higherOrder);
                return FunctionExpressionFactory.Instance.Create(
                    parameter, body, this.Range);
            }
        }

        protected override IExpression Reduce(IReduceContext context)
        {
            var parameter = context.Reduce(this.Parameter);
            var body = context.Reduce(this.Body);

            if (object.ReferenceEquals(this.Parameter, parameter) &&
                object.ReferenceEquals(this.Body, body))
            {
                return this;
            }
            else
            {
                return FunctionExpressionFactory.Instance.Create(
                    parameter,
                    body,
                    () => this.HigherOrder,
                    this.Range);
            }
        }

        public IExpression Call(IReduceContext context, IExpression argument) =>
            (this.Parameter is IBoundVariableTerm bound ?
                context.Bind(bound, argument) :
                context).
            Reduce(this.Body);

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            new[] { context.GetXml(this.Parameter), context.GetXml(this.Body) };

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                $"{context.GetPrettyString(this.Parameter)} -> {context.GetPrettyString(this.Body)}");
        
        public static FunctionExpression Create(
            IExpression parameter, IExpression body, IFunctionExpression higherOrder, TextRange range) =>
            (FunctionExpression)FunctionExpressionFactory.Instance.Create(
                parameter, body, higherOrder, range);

        public static FunctionExpression Create(
            IExpression parameter, IExpression body, TextRange range) =>
            (FunctionExpression)FunctionExpressionFactory.Instance.Create(
                parameter, body, range);
    }

    [DebuggerStepThrough]
    public static class FunctionExpressionExtension
    {
        public static void Deconstruct(
            this IFunctionExpression function,
            out IExpression parameter,
            out IExpression body)
        {
            parameter = function.Parameter;
            body = function.Body;
        }
    }
}
