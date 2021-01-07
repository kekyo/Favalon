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
    internal interface IMethodExpression :
        ICallableExpression
    {
        IEnumerable GetXmlValues(IXmlRenderContext context);
        string GetPrettyString(IPrettyStringContext context);
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
            if (reducedArgument is IConstantTerm({ } value))
            {
                // TODO: insert better position directly with instance/static/extension method knowleges.
                this.Arguments.Add(value);
                if (this.Method is MethodBinderExpression binder)
                {
                    // BAD: Mutation because reduce costs.
                    // TODO: It's mutable, will break when this expression is multiple used.
                    //       ex: partially function application.
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
}
