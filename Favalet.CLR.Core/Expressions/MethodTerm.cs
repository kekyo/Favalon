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
using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;
using Favalet.Ranges;

namespace Favalet.Expressions
{
    public interface IMethodTerm :
        ITerm, ICallableExpression
    {
        MethodBase RuntimeMethod { get; }
    }

    public sealed class MethodTerm :
        Expression, IMethodTerm
    {
        private readonly LazySlim<IExpression> higherOrder;

        public readonly MethodBase RuntimeMethod;

        [DebuggerStepThrough]
        private MethodTerm(MethodBase runtimeMethod, LazySlim<IExpression> higherOrder, TextRange range) :
            base(range)
        {
            this.RuntimeMethod = runtimeMethod;
            this.higherOrder = higherOrder;
        }

        public override IExpression HigherOrder
        {
            [DebuggerStepThrough]
            get => this.higherOrder.Value;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        MethodBase IMethodTerm.RuntimeMethod
        {
            [DebuggerStepThrough]
            get => this.RuntimeMethod;
        }

        public override int GetHashCode() =>
            this.RuntimeMethod.GetHashCode();

        public bool Equals(IMethodTerm rhs) =>
            this.RuntimeMethod.Equals(rhs.RuntimeMethod);

        public override bool Equals(IExpression? other) =>
            other is IMethodTerm rhs && this.Equals(rhs);
        
        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            new MethodTerm(
                this.RuntimeMethod,
                LazySlim.Create(context.MakeRewritableHigherOrder(this.HigherOrder)),
                this.Range);

        protected override IExpression Infer(IInferContext context) =>
            this;

        protected override IExpression Fixup(IFixupContext context) =>
            this;

        protected override IExpression Reduce(IReduceContext context) =>
            this;

        public IExpression Call(IReduceContext context, IExpression argument)
        {
            if (argument is IConstantTerm constant)
            {
                if (this.RuntimeMethod is ConstructorInfo constructor)
                {
                    var result = constructor.Invoke(new[] { constant.Value });
                    return ConstantTerm.From(result, this.Range);
                }
                else
                {
                    var method = (MethodInfo)this.RuntimeMethod;
                    var result = method.Invoke(null, new[] { constant.Value });
                    return ConstantTerm.From(result, this.Range);
                }
            }
            else
            {
                throw new ArgumentException(argument.GetPrettyString(PrettyStringTypes.Readable));
            }
        }

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            new[] { new XAttribute("name", this.RuntimeMethod.GetReadableName()) };

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                this.RuntimeMethod.GetReadableName());

        [DebuggerStepThrough]
        private static IExpression CreateHigherOrder(MethodBase method, TextRange range)
        {
            var parameterType0 = method.GetParameters()[0].ParameterType;
            var returnType = (method is MethodInfo mi ? mi.ReturnType! : method.DeclaringType!) ?? typeof(void);
            return FunctionExpression.Create(
                TypeTerm.From(parameterType0,TextRange.From(parameterType0)),
                TypeTerm.From(returnType, TextRange.From(returnType)),
                range);
        }

        [DebuggerStepThrough]
        public static MethodTerm From(MethodBase method, TextRange range) =>
            new MethodTerm(method, LazySlim.Create(() => CreateHigherOrder(method, TextRange.Unknown)), range);  // TODO: range
    }

    public static class MethodTermExtension
    {
        [DebuggerStepThrough]
        public static void Deconstruct(
            this IMethodTerm method,
            out MethodBase runtimeMethod) =>
            runtimeMethod = method.RuntimeMethod;
    }
}
