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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Favalet.Ranges;

namespace Favalet.Expressions
{
    public interface IFunctionExpression :
        IExpression
    {
        IExpression Parameter { get; }

        IExpression Result { get; }
    }

    public abstract class FunctionExpressionBase :
        Expression, IFunctionExpression, IPairExpression
    {
        #region Factory
        [DebuggerStepThrough]
        private protected abstract class FunctionExpressionFactoryBase
        {
            private readonly IFunctionExpression fourth;

            private protected FunctionExpressionFactoryBase() =>
                this.fourth = this.OnCreate(
                    FourthTerm.Instance,
                    FourthTerm.Instance,
                    DeadEndTerm.Instance,
                    TextRange.Unknown);

            protected abstract IFunctionExpression OnCreate(
                IExpression parameter, IExpression result, IExpression higherOrder, TextRange range);
            
            public IExpression Create(
                IExpression parameter, IExpression result, Func<IExpression> higherOrder, TextRange range)
            {
                switch (parameter, result)
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
                            result,
                            DeadEndTerm.Instance,
                            range);
                    default:
                        return this.OnCreate(
                            parameter,
                            result,
                            higherOrder(),
                            range);
                };
            }

            public IFunctionExpression Create(
                IExpression parameter, IExpression result, IFunctionExpression higherOrder, TextRange range) =>
                (IFunctionExpression)this.Create(
                    parameter,
                    result,
                    () => higherOrder,
                    range);

            private IExpression CreateRecursivity(
                IExpression parameter, IExpression result, TextRange range) =>
                this.Create(
                    parameter,
                    result,
                    () => (parameter, result) switch
                    {
                        (UnspecifiedTerm _, UnspecifiedTerm _) =>
                            DeadEndTerm.Instance,
                        (UnspecifiedTerm _, _) =>
                            this.CreateRecursivity(UnspecifiedTerm.Instance, result.HigherOrder, TextRange.Unknown),
                        (_, UnspecifiedTerm _) =>
                            this.CreateRecursivity(parameter.HigherOrder, UnspecifiedTerm.Instance, TextRange.Unknown),
                        _ =>
                            this.CreateRecursivity(parameter.HigherOrder, result.HigherOrder, TextRange.Unknown)
                    },
                    range);

            public IFunctionExpression Create(
                IExpression parameter, IExpression result, TextRange range) =>
                (IFunctionExpression)this.CreateRecursivity(parameter, result, range);
        }
        #endregion
        
        private protected abstract FunctionExpressionFactoryBase Factory { get; }
        
        public readonly IExpression Parameter;
        public readonly IExpression Result;

        [DebuggerStepThrough]
        private protected FunctionExpressionBase(
            IExpression parameter, IExpression result, IExpression higherOrder, TextRange range) :
            base(range)
        {
            this.Parameter = parameter;
            this.Result = result;
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
        IExpression IFunctionExpression.Result
        {
            [DebuggerStepThrough]
            get => this.Result;
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
            get => this.Result;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Type IPairExpression.IdentityType
        {
            [DebuggerStepThrough]
            get => typeof(IFunctionExpression);
        }

        [DebuggerStepThrough]
        IExpression IPairExpression.Create(IExpression left, IExpression right, TextRange range) =>
            this.Factory.Create(left, right, range);

        public override int GetHashCode() =>
            this.Parameter.GetHashCode() ^ this.Result.GetHashCode();

        public bool Equals(IFunctionExpression rhs) =>
            this.Parameter.Equals(rhs.Parameter) &&
            this.Result.Equals(rhs.Result);

        public override bool Equals(IExpression? other) =>
            other is IFunctionExpression rhs && this.Equals(rhs);

        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            this.Factory.Create(
                context.MakeRewritable(this.Parameter),
                context.MakeRewritable(this.Result),
                () => context.MakeRewritableHigherOrder(this.HigherOrder),
                this.Range);

        protected override IExpression Infer(IInferContext context)
        {
            var parameter = context.Infer(this.Parameter);
            var result = context.Infer(this.Result);

            // Recursive inferring exit rule.
            if (parameter is FourthTerm || result is FourthTerm ||
                parameter.HigherOrder is DeadEndTerm || result.HigherOrder is DeadEndTerm)
            {
                if (object.ReferenceEquals(this.Parameter, parameter) &&
                    object.ReferenceEquals(this.Result, result) &&
                    this.HigherOrder is DeadEndTerm)
                {
                    return this;
                }
                else
                {
                    return this.Factory.Create(
                        parameter,
                        result,
                        () => DeadEndTerm.Instance,
                        this.Range);
                }
            }
            else
            {
                var higherOrder = context.Infer(this.HigherOrder);

                var functionHigherOrder = this.Factory.Create(
                    parameter.HigherOrder, result.HigherOrder, this.Range);

                context.Unify(functionHigherOrder, higherOrder, true);

                if (object.ReferenceEquals(this.Parameter, parameter) &&
                    object.ReferenceEquals(this.Result, result) &&
                    object.ReferenceEquals(this.HigherOrder, higherOrder))
                {
                    return this;
                }
                else
                {
                    return this.Factory.Create(
                        parameter,
                        result,
                        () => higherOrder,
                        this.Range);
                }
            }
        }

        protected override IExpression Fixup(IFixupContext context)
        {
            var parameter = context.Fixup(this.Parameter);
            var result = context.Fixup(this.Result);
            var higherOrder = context.FixupHigherOrder(this.HigherOrder);

            if (object.ReferenceEquals(this.Parameter, parameter) &&
                object.ReferenceEquals(this.Result, result) &&
                object.ReferenceEquals(this.HigherOrder, higherOrder))
            {
                return this;
            }
            else if (higherOrder is IFunctionExpression functionExpression)
            {
                return this.Factory.Create(parameter, result, functionExpression, this.Range);
            }
            else
            {
                // TODO: Apply fixed up higher order.
                //return InternalCreate(parameter, result, () => higherOrder);
                return this.Factory.Create(parameter, result, this.Range);
            }
        }

        protected override IExpression Reduce(IReduceContext context)
        {
            var parameter = context.Reduce(this.Parameter);
            var result = context.Reduce(this.Result);

            if (object.ReferenceEquals(this.Parameter, parameter) &&
                object.ReferenceEquals(this.Result, result))
            {
                return this;
            }
            else
            {
                return this.Factory.Create(
                    parameter,
                    result,
                    () => this.HigherOrder,
                    this.Range);
            }
        }

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            new[] { context.GetXml(this.Parameter), context.GetXml(this.Result) };

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                $"{context.GetPrettyString(this.Parameter)} -> {context.GetPrettyString(this.Result)}");
    }

    [DebuggerStepThrough]
    public static class FunctionExpressionExtension
    {
        public static void Deconstruct(
            this IFunctionExpression function,
            out IExpression parameter,
            out IExpression result)
        {
            parameter = function.Parameter;
            result = function.Result;
        }
    }
}
