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
using System.Xml.Linq;

namespace Favalet.Expressions
{
    public interface IConstantTerm : ITerm
    {
        object Value { get; }
    }

    public sealed class ConstantTerm :
        Expression, IConstantTerm
    {
        private readonly LazySlim<IExpression> higherOrder;

        public readonly object Value;

        [DebuggerStepThrough]
        private ConstantTerm(object value, LazySlim<IExpression> higherOrder, TextRange range) :
            base(range)
        {
            this.Value = value;
            this.higherOrder = higherOrder;
        }

        public override IExpression HigherOrder
        {
            [DebuggerStepThrough]
            get => higherOrder.Value;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object IConstantTerm.Value
        {
            [DebuggerStepThrough]
            get => this.Value;
        }

        public override int GetHashCode() =>
            this.Value.GetHashCode();

        public bool Equals(IConstantTerm rhs) =>
            this.Value.Equals(rhs.Value);

        public override bool Equals(IExpression? other) =>
            other is IConstantTerm rhs && this.Equals(rhs);
        
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

        private string StringValue
        {
            [DebuggerStepThrough]
            get => this.Value switch
            {
                string value => $"\"{value}\"",
                _ => this.Value.ToString()
            };
        }

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            new[] { new XAttribute("value", this.StringValue) };

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                this.StringValue);

        [DebuggerStepThrough]
        public static ConstantTerm From(object? value, TextRange range) =>
            // TODO: null value
            new ConstantTerm(value!, LazySlim.Create(() =>
                (IExpression)TypeTerm.From(value!.GetType(), TextRange.Unknown)),
                range);
    }

    [DebuggerStepThrough]
    public static class ConstantTermExtension
    {
        public static void Deconstruct(this IConstantTerm constant, out object value) =>
            value = constant.Value;
    }
}
