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
        private readonly bool ignoreThisOrder;

        public readonly MethodBase RuntimeMethod;

        [DebuggerStepThrough]
        private MethodTerm(
            MethodBase runtimeMethod, Type parameterType, bool ignoreThisOrder, TextRange range) :
            base(range)
        {
            this.RuntimeMethod = runtimeMethod;
            this.ignoreThisOrder = ignoreThisOrder;
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

        private object? Call(object[] arguments)
        {
            Debug.Assert(arguments.Length >= 1);

            // TODO: no args (unit?)

            switch (this.RuntimeMethod, this.RuntimeMethod.IsStatic, this.ignoreThisOrder)
            {
                // Constructor
                case (ConstructorInfo constructor, false, _):
                    return constructor.Invoke(arguments);
                // Instance method (Functional standard this order)
                case (MethodInfo method, false, false):
                {
                    var index = arguments.Length - 1;
                    var args = arguments.Take(index).Memoize();
                    return method.Invoke(arguments[index], args);
                }
                // Instance method (.NET standard this order)
                case (MethodInfo method, false, true):
                {
                    var args = arguments.Skip(1).Memoize();
                    return method.Invoke(arguments[0], args);
                }
                // Extension method
                case (MethodInfo method, true, false) when method.IsDefined(typeof(ExtensionAttribute), false):
                {
                    var index = arguments.Length - 1;
                    var args = arguments.Take(index).Prepend(arguments[index]).Memoize();
                    return method.Invoke(null, args);
                }
                // Static method
                case (MethodInfo method, true, _):
                    return method.Invoke(null, arguments);
                default:
                    throw new InvalidOperationException();
            }
        }

        public IExpression Call(IReduceContext context, IExpression reducedArgument)
        {
            object? result;
            switch (reducedArgument)
            {
                case IConstantTerm constant:
                    result = this.Call(new[] {constant.Value});
                    break;
                case MethodPartialClosureExpression closure:
                    result = this.Call(closure.Arguments.Memoize());
                    break;
                default:
                    throw new ArgumentException(reducedArgument.GetPrettyString(PrettyStringTypes.Readable));
            }

            if (this.RuntimeMethod is MethodInfo method &&
                method.ReturnType == typeof(void))
            {
                Debug.Assert(result == null);
                return UnitTerm.Instance;
            }
            else
            {
                return ConstantTerm.From(result, this.Range);
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

        private static IEnumerable<(string name, Type type)> GetNormalizedParameters(
            MethodBase method, bool ignoreThisOrder)
        {
            var parameters = method.GetParameters().
                Select(p => (p.Name, p.ParameterType));
            switch (method, method.IsStatic, ignoreThisOrder)
            {
                // Constructor
                case (ConstructorInfo _, false, _):
                    return parameters!;
                // Instance method (Functional standard this order)
                case (MethodInfo _, false, false):
                    return parameters!.Append(("this", method.DeclaringType))!;
                // Instance method (.NET standard this order)
                case (MethodInfo _, false, true):
                    return parameters!.Prepend(("this", method.DeclaringType))!;
                // Extension method
                case (MethodInfo _, true, false) when method.IsDefined(typeof(ExtensionAttribute), false):
                    var ps = parameters!.Skip(1);
                    return ps.Append(parameters!.First())!;
                // Static method
                case (MethodInfo _, true, _):
                    return parameters!;
                default:
                    throw new InvalidOperationException();
            }
        }

        private static IExpression From(
            MethodBase method, TextRange range, bool ignoreThisOrder)
        {
            var parameters = GetNormalizedParameters(method, ignoreThisOrder).
                Reverse().
                Memoize();
            var result = parameters.
                Skip(1).
                Aggregate(
                    (IMethodExpression)new MethodTerm(method, parameters[0].type, ignoreThisOrder, range),
                    (agg, p) => new MethodBinderExpression(agg, p.name, p.type, CLRGenerator.TextRange(method)));
            return result;
        }

        [DebuggerStepThrough]
        public static IExpression From(MethodBase method, TextRange range) =>
            From(method, range, false);

        public static IExpression From(Delegate d, TextRange range) =>
            (d.Target != null) ?
                ApplyExpression.Create(
                    From(d.GetMethodInfo(), range, true),
                    ConstantTerm.From(d.Target, TextRange.Unknown),
                    range) :
                From(d.GetMethodInfo(), range, false);
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

        public IExpression Call(IReduceContext context, IExpression reducedArgument)
        {
            if (reducedArgument is IConstantTerm constant)
            {
                var closure = new MethodPartialClosureExpression(this.Method);
                closure.Arguments.Add(constant.Value);
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
        
        public IExpression Call(IReduceContext context, IExpression reducedArgument)
        {
            if (reducedArgument is IConstantTerm constant)
            {
                // TODO: insert better position directly with instance/static/extension method knowleges.
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
                throw new ArgumentException(reducedArgument.GetPrettyString(PrettyStringTypes.Readable));
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
