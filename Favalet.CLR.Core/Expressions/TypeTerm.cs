﻿/////////////////////////////////////////////////////////////////////////////////////////////////
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
using System;
using System.Collections;
using System.Diagnostics;
using System.Xml.Linq;

namespace Favalet.Expressions
{
    public interface ITypeTerm :
        ITerm
    {
        Type RuntimeType { get; }
    }

    public sealed class TypeTerm :
        Expression, ITypeTerm
    {
        public readonly Type RuntimeType;

        [DebuggerStepThrough]
        private TypeTerm(Type runtimeType) =>
            this.RuntimeType = runtimeType;

        public override IExpression HigherOrder
        {
            [DebuggerStepThrough]
            get => Generator.Kind();
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Type ITypeTerm.RuntimeType
        {
            [DebuggerStepThrough]
            get => this.RuntimeType;
        }

        public override int GetHashCode() =>
            this.RuntimeType.GetHashCode();

        public bool Equals(ITypeTerm rhs) =>
            this.RuntimeType.Equals(rhs.RuntimeType);

        public override bool Equals(IExpression? other) =>
            other is ITypeTerm rhs && this.Equals(rhs);

        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            this;

        protected override IExpression Infer(IInferContext context) =>
            this;

        protected override IExpression Fixup(IFixupContext context) =>
            this;

        protected override IExpression Reduce(IReduceContext context) =>
            this;

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            new[] { new XAttribute("name", this.RuntimeType.GetReadableName()) };

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                this.RuntimeType.GetReadableName());

        [DebuggerStepThrough]
        public static ITerm From(Type type)
        {
            if (type.Equals(typeof(object).GetType()))
            {
                return Generator.Kind();
            }
            else
            {
                return new TypeTerm(type);
            }
        }
    }

    public static class TypeTermExtension
    {
        [DebuggerStepThrough]
        public static void Deconstruct(
            this ITypeTerm type,
            out Type runtimeType) =>
            runtimeType = type.RuntimeType;
    }
}
