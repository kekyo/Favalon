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

namespace Favalet.Expressions
{
    public interface IMethodTerm :
        ITerm, ICallableExpression
    {
        MethodBase RuntimeMethod { get; }
    }

    internal interface IMethodExpression :
        ICallableExpression
    {
        IEnumerable GetXmlValues(IXmlRenderContext context);
        string GetPrettyString(IPrettyStringContext context);
    }

    public sealed class MethodTerm :
        Expression, IMethodTerm, IMethodExpression
    {
        private readonly LazySlim<IExpression> higherOrder;

        public readonly MethodBase RuntimeMethod;

        [DebuggerStepThrough]
        private MethodTerm(
            MethodBase runtimeMethod, Type parameterType, TextRange range) :
            base(range)
        {
            this.RuntimeMethod = runtimeMethod;
            this.higherOrder = LazySlim.Create<IExpression>(() =>
                LambdaExpression.Create(
                    TypeTerm.From(parameterType, this.Range),
                    TypeTerm.From(
                        this.RuntimeMethod is MethodInfo mi ? mi.ReturnType : this.RuntimeMethod.DeclaringType!,
                        this.Range),
                    this.Range));
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

        private object? Call(object?[] arguments)
        {
            Debug.Assert(arguments.Length >= 1);

            // TODO: no args (unit?)

            if (this.RuntimeMethod is ConstructorInfo constructor)
            {
                var result = constructor.Invoke(arguments);
                return result;
            }
            else if (this.RuntimeMethod.IsStatic)
            {
                var method = (MethodInfo)this.RuntimeMethod;
                if (method.IsDefined(typeof(ExtensionAttribute), false))
                {
                    var index = arguments.Length - 1;
                    var args = arguments.Take(index).Prepend(arguments[index]).ToArray();
                    var result = method.Invoke(null, args);
                    return result;
                }
                else
                {
                    var result = method.Invoke(null, arguments);
                    return result;
                }
            }
            else
            {
                var method = (MethodInfo) this.RuntimeMethod;
                var args = arguments.Skip(1).ToArray();
                var result = method.Invoke(arguments[0], args);
                return result;
            }
        }

        public IExpression Call(IReduceContext context, IExpression argument)
        {
            switch (argument)
            {
                case IConstantTerm constant:
                    return ConstantTerm.From(this.Call(new[] {constant.Value}), this.Range);
                case MethodPartialClosureExpression closure:
                    return ConstantTerm.From(this.Call(closure.Arguments.ToArray()), this.Range);
                default:
                    throw new ArgumentException(argument.GetPrettyString(PrettyStringTypes.Readable));
            }
        }

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            new[] {new XAttribute("name", this.RuntimeMethod.GetReadableName())};

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                this.RuntimeMethod.GetReadableName());

        [DebuggerStepThrough]
        IEnumerable IMethodExpression.GetXmlValues(IXmlRenderContext context) =>
            this.GetXmlValues(context);
        [DebuggerStepThrough]
        string IMethodExpression.GetPrettyString(IPrettyStringContext context) =>
            this.GetPrettyString(context);

        [DebuggerStepThrough]
        private static IEnumerable<(string name, Type type)> GetNormalizedParameters(MethodBase method)
        {
            var parameters = method.GetParameters().
                Select(p => (p.Name, p.ParameterType));
            switch (method, method.IsStatic)
            {
                case (ConstructorInfo _, false):
                    return parameters!;
                case (MethodInfo _, false):
                    return parameters!.Append(("this", method.DeclaringType))!;
                case (MethodInfo _, true) when method.IsDefined(typeof(ExtensionAttribute), false):
                    var ps = parameters!.Skip(1);
                    return ps.Append(parameters!.First())!;
                case (MethodInfo _, true):
                    return parameters!;
                default:
                    throw new InvalidOperationException();
            }
        }

        [DebuggerStepThrough]
        public static IExpression From(MethodBase method, TextRange range)
        {
            var parameters = GetNormalizedParameters(method).
                Reverse().
                ToArray();
            var result = parameters.
                Skip(1).
                Aggregate(
                    (IMethodExpression)new MethodTerm(method, parameters[0].type, range),
                    (agg, p) => new MethodBinderExpression(agg, p.name, p.type, CLRGenerator.TextRange(method)));
            return result;
        }
    }

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

        public IExpression Call(IReduceContext context, IExpression argument)
        {
            if (argument is IConstantTerm constant)
            {
                var closure = new MethodPartialClosureExpression(this.Method);
                closure.Arguments.Add(constant.Value);
                return closure;
            }
            else
            {
                throw new ArgumentException(argument.GetPrettyString(PrettyStringTypes.Readable));
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

    internal sealed class MethodPartialClosureExpression :
        Expression, ICallableExpression
    {
        public readonly List<object> Arguments = new();
        public IMethodExpression Method;

        public MethodPartialClosureExpression(IMethodExpression method) :
            base(TextRange.Unknown) =>
            this.Method = method;

        public override IExpression HigherOrder =>
            this.Method.HigherOrder;

        [DebuggerStepThrough]
        protected override string GetTypeName() =>
            "Closure";

        public override int GetHashCode() =>
            throw new InvalidOperationException();

        public bool Equals(MethodBinderExpression rhs) =>
            throw new InvalidOperationException();

        public override bool Equals(IExpression? other) =>
            throw new InvalidOperationException();
                
        protected override IExpression Transpose(ITransposeContext context) =>
            throw new InvalidOperationException();

        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            throw new InvalidOperationException();

        protected override IExpression Infer(IInferContext context) =>
            throw new InvalidOperationException();

        protected override IExpression Fixup(IFixupContext context) =>
            throw new InvalidOperationException();

        protected override IExpression Reduce(IReduceContext context) =>
            throw new InvalidOperationException();
        
        public IExpression Call(IReduceContext context, IExpression argument)
        {
            if (argument is IConstantTerm constant)
            {
                this.Arguments.Add(constant.Value);
                if (this.Method is MethodBinderExpression binder)
                {
                    // Mutation because reduce costs.
                    this.Method = binder.Method;
                    return this;
                }
                else
                {
                    return this.Method.Call(context, this);
                }
            }
            else
            {
                throw new ArgumentException(argument.GetPrettyString(PrettyStringTypes.Readable));
            }
        }

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            this.Method.GetXmlValues(context);

        protected override string GetPrettyString(IPrettyStringContext context) =>
            this.Method.GetPrettyString(context);
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
