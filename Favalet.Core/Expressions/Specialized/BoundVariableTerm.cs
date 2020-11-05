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
using System.Collections;
using System.Diagnostics;
using System.Xml.Linq;
using Favalet.Ranges;

namespace Favalet.Expressions.Specialized
{
    public interface IBoundVariableTerm :
        ITerm
    {
        string Symbol { get; }
    }

    public sealed class BoundVariableTerm :
        Expression, IBoundVariableTerm
    {
        public readonly string Symbol;

        [DebuggerStepThrough]
        private BoundVariableTerm(string symbol, IExpression higherOrder, TextRange range) :
            base(range)
        {
            this.HigherOrder = higherOrder;
            this.Symbol = symbol;
        }

        public override IExpression HigherOrder { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string IBoundVariableTerm.Symbol
        {
            [DebuggerStepThrough]
            get => this.Symbol;
        }

        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            new BoundVariableTerm(
                this.Symbol,
                context.MakeRewritableHigherOrder(this.HigherOrder),
                this.Range);

        protected override IExpression Infer(IInferContext context)
        {
            var higherOrder = context.Infer(this.HigherOrder);

            if (object.ReferenceEquals(this.HigherOrder, higherOrder))
            {
                return this;
            }
            else
            {
                return new BoundVariableTerm(this.Symbol, higherOrder, this.Range);
            }
        }

        protected override IExpression Fixup(IFixupContext context)
        {
            var higherOrder = context.FixupHigherOrder(this.HigherOrder);

            if (object.ReferenceEquals(this.HigherOrder, higherOrder))
            {
                return this;
            }
            else
            {
                return new BoundVariableTerm(this.Symbol, higherOrder, this.Range);
            }
        }

        protected override IExpression Reduce(IReduceContext context) =>
            this;
        
        public override int GetHashCode() =>
            this.Symbol.GetHashCode();

        public bool Equals(IBoundVariableTerm rhs) =>
            this.Symbol.Equals(rhs.Symbol);

        public override bool Equals(IExpression? other) =>
            other is IBoundVariableTerm rhs && this.Equals(rhs);

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            new[] { new XAttribute("symbol", this.Symbol) };

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                this.Symbol);

        [DebuggerStepThrough]
        public static BoundVariableTerm Create(string symbol, IExpression higherOrder, TextRange range) =>
            new BoundVariableTerm(symbol, higherOrder, range);
        [DebuggerStepThrough]
        public static BoundVariableTerm Create(string symbol, TextRange range) =>
            new BoundVariableTerm(symbol, UnspecifiedTerm.Instance, range);
    }
}
