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
using Favalet.Ranges;
using System.Collections;
using System.Diagnostics;
using System.Linq;

namespace Favalet.Expressions.Operators
{
    public sealed class LambdaOperatorExpression :
        Expression, ICallableExpression
    {
        private static readonly LambdaExpression higherOrder =
            LambdaExpression.Create(
                UnspecifiedTerm.Instance,
                LambdaExpression.Create(
                    UnspecifiedTerm.Instance,
                    UnspecifiedTerm.Instance,
                    TextRange.Unknown),
                TextRange.Unknown);

        private LambdaOperatorExpression(IExpression higherOrder, TextRange range) :
            base(range) =>
            this.HigherOrder = higherOrder;

        public override IExpression HigherOrder { get; }

        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            new LambdaOperatorExpression(
                context.MakeRewritableHigherOrder(this.HigherOrder),
                this.Range);

        protected override IExpression Infer(IInferContext context)
        {
            var higherOrder = context.Infer(this.HigherOrder);

            if (object.ReferenceEquals(higherOrder, this.HigherOrder))
            {
                return this;
            }
            else
            {
                return new LambdaOperatorExpression(higherOrder, this.Range);
            }
        }

        protected override IExpression Fixup(IFixupContext context)
        {
            var higherOrder = context.Fixup(this.HigherOrder);

            if (object.ReferenceEquals(higherOrder, this.HigherOrder))
            {
                return this;
            }
            else
            {
                return new LambdaOperatorExpression(higherOrder, this.Range);
            }
        }

        protected override IExpression Reduce(IReduceContext context)
        {
            var higherOrder = context.Reduce(this.HigherOrder);

            if (object.ReferenceEquals(higherOrder, this.HigherOrder))
            {
                return this;
            }
            else
            {
                return new LambdaOperatorExpression(higherOrder, this.Range);
            }
        }

        public IExpression Call(IReduceContext context, IExpression argument)
        {
            var target = argument is IApplyExpression({} parameter, {} body) ?
                LambdaExpression.Create(parameter, body, this.Range) :
                argument;

            return context.Reduce(target);
        }

        public bool Equals(LambdaOperatorExpression rhs) =>
            rhs != null;

        public override bool Equals(IExpression? other) =>
            other is LambdaOperatorExpression;

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            Enumerable.Empty<object>();

        protected override string GetPrettyString(IPrettyStringContext context) =>
            "->";

        public static readonly LambdaOperatorExpression Instance =
            new LambdaOperatorExpression(higherOrder, TextRange.Unknown);  // TODO: range
    }
}
