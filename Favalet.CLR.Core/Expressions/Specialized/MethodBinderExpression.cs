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
using Favalet.Internal;
using Favalet.Ranges;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Favalet.Expressions.Specialized;

namespace Favalet.Expressions.Specialized
{
    internal sealed class MethodBinderExpression :
        Expression, IMethodExpression
    {
        private readonly LazySlim<IExpression> higherOrder;

        public readonly string ParameterName;
        public readonly IMethodExpression Method;

        [DebuggerStepThrough]
        public MethodBinderExpression(
            IMethodExpression method,
            string parameterName,
            Type parameterType,
            TextRange range) :
            base(range)
        {
            this.ParameterName = parameterName;
            this.Method = method;
            this.higherOrder = LazySlim.Create<IExpression>(() =>
                LambdaExpression.Create(
                    TypeTerm.From(parameterType, this.Range),
                    this.Method.HigherOrder,
                    this.Range));
        }
 
        public override IExpression HigherOrder
        {
            [DebuggerStepThrough]
            get => this.higherOrder.Value;
        }

        public override int GetHashCode() =>
            this.ParameterName.GetHashCode() ^ this.Method.GetHashCode();

        public bool Equals(MethodBinderExpression rhs) =>
            this.ParameterName.Equals(rhs.ParameterName) && 
            this.Method.Equals(rhs.Method);

        public override bool Equals(IExpression? other) =>
            other is MethodBinderExpression rhs && this.Equals(rhs);
                
        protected override IExpression Transpose(ITransposeContext context) =>
            this;

        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            this;

        protected override IExpression Infer(IInferContext context) =>
            this;

        protected override IExpression Fixup(IFixupContext context) =>
            this;

        protected override IExpression Reduce(IReduceContext context) =>
            this;

        public IExpression Call(IReduceContext context, IExpression reducedArgument)
        {
            if (reducedArgument is IConstantTerm({ } value))
            {
                var closure = new MethodPartialClosureExpression(this.Method);
                closure.Arguments.Add(value);
                return closure;
            }
            else
            {
                throw new ArgumentException(reducedArgument.GetPrettyString(PrettyStringTypes.Readable));
            }
        }

        private static IMethodExpression GetMethod(IMethodExpression method) =>
            method switch
            {
                MethodTerm m => m,
                MethodBinderExpression mbe => GetMethod(mbe.Method),
                _ => throw new InvalidOperationException()
            };

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            GetMethod(this.Method).GetXmlValues(context);

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.GetPrettyString(GetMethod(this.Method));
        
        [DebuggerStepThrough]
        IEnumerable IMethodExpression.GetXmlValues(IXmlRenderContext context) =>
            this.GetXmlValues(context);
        [DebuggerStepThrough]
        string IMethodExpression.GetPrettyString(IPrettyStringContext context) =>
            this.GetPrettyString(context);
    }
}
