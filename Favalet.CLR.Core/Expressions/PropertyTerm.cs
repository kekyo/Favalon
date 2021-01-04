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
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

namespace Favalet.Expressions
{
    public interface IPropertyTerm :
        ITerm
    {
        PropertyInfo RuntimeProperty { get; }
    }

    public sealed class PropertyTerm :
        Expression, IPropertyTerm
    {
        public readonly PropertyInfo RuntimeProperty;

        [DebuggerStepThrough]
        private PropertyTerm(PropertyInfo runtimeProperty, IExpression higherOrder, TextRange range) :
            base(range)
        {
            this.RuntimeProperty = runtimeProperty;
            this.HigherOrder = higherOrder;
        }

        public override IExpression HigherOrder { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        PropertyInfo IPropertyTerm.RuntimeProperty
        {
            [DebuggerStepThrough]
            get => this.RuntimeProperty;
        }

        public override int GetHashCode() =>
            this.RuntimeProperty.GetHashCode();

        public bool Equals(IPropertyTerm rhs) =>
            this.RuntimeProperty.Equals(rhs.RuntimeProperty);

        public override bool Equals(IExpression? other) =>
            other is IPropertyTerm rhs && this.Equals(rhs);
        
        protected override IExpression Transpose(ITransposeContext context) =>
            this;

        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            this;

        protected override IExpression Infer(IInferContext context) =>
            this;

        protected override IExpression Fixup(IFixupContext context) =>
            this;

        protected override IExpression Reduce(IReduceContext context) =>
            ConstantTerm.From(
                this.RuntimeProperty.GetValue(null, ArrayEx.Empty<object>()),
                this.Range);

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            new[] { new XAttribute("name", this.RuntimeProperty.GetReadableName()) };

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                this.RuntimeProperty.GetReadableName());

        [DebuggerStepThrough]
        public static IExpression From(PropertyInfo runtimeProperty, TextRange range)
        {
            var isStatic =
                (runtimeProperty.GetGetMethod() ?? runtimeProperty.GetSetMethod())?.IsStatic ?? true;

            if (isStatic)
            {
                return new PropertyTerm(
                    runtimeProperty,
                    TypeTerm.From(runtimeProperty.PropertyType, range),
                    range);  // TODO: range
            }
            else
            {
                // TODO: setter
                return MethodTerm.From(runtimeProperty.GetGetMethod()!, range);
            }
        }
    }

    public static class PropertyTermExtension
    {
        [DebuggerStepThrough]
        public static void Deconstruct(
            this IPropertyTerm property,
            out PropertyInfo runtimeProperty) =>
            runtimeProperty = property.RuntimeProperty;
    }
}
