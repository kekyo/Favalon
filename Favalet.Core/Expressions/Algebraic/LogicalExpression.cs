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
using System.Collections;
using System.Diagnostics;
using Favalet.Internal;
using Favalet.Ranges;

namespace Favalet.Expressions.Algebraic
{
    public interface ILogicalExpression : IExpression
    {
        IExpression Operand { get; }
    }

    public sealed class LogicalExpression :
        Expression, ILogicalExpression
    {
        public readonly IExpression Operand;

        [DebuggerStepThrough]
        private LogicalExpression(
            IExpression operand, IExpression higherOrder, TextRange range) :
            base(range)
        {
            this.HigherOrder = higherOrder;
            this.Operand = operand;
        }

        public override IExpression HigherOrder { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IExpression ILogicalExpression.Operand
        {
            [DebuggerStepThrough]
            get => this.Operand;
        }

        public override int GetHashCode() =>
            this.Operand.GetHashCode();

        public bool Equals(ILogicalExpression rhs) =>
            LogicalCalculator.Instance.Equals(this.Operand, rhs.Operand);

        public override bool Equals(IExpression? other) =>
            other is ILogicalExpression rhs && this.Equals(rhs);

        protected override IExpression Transpose(ITransposeContext context) =>
            new LogicalExpression(
                context.Transpose(this.Operand),
                context.Transpose(this.HigherOrder),
                this.Range);

        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            new LogicalExpression(
                context.MakeRewritable(this.Operand),
                context.MakeRewritableHigherOrder(this.HigherOrder),
                this.Range);

        protected override IExpression Infer(IInferContext context)
        {
            var higherOrder = context.Infer(this.HigherOrder);
            var operand = context.Infer(this.Operand);

            context.Unify(operand.HigherOrder, higherOrder, false);

            if (object.ReferenceEquals(this.HigherOrder, higherOrder) &&
                object.ReferenceEquals(this.Operand, operand))
            {
                return this;
            }
            else
            {
                return new LogicalExpression(operand, higherOrder, this.Range);
            }
        }

        protected override IExpression Fixup(IFixupContext context)
        {
            var higherOrder = context.Fixup(this.HigherOrder);
            var operand = context.Fixup(this.Operand);

            if (object.ReferenceEquals(this.HigherOrder, higherOrder) &&
                object.ReferenceEquals(this.Operand, operand))
            {
                return this;
            }
            else
            {
                return new LogicalExpression(operand, higherOrder, this.Range);
            }
        }

        protected override IExpression Reduce(IReduceContext context) =>
            LogicalCalculator.Instance.Reduce(context.Reduce(this.Operand));

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            new[] { context.GetXml(this.Operand) };

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                context.GetPrettyString(this.Operand));

        [DebuggerStepThrough]
        public static LogicalExpression Create(
            IExpression operand, IExpression higherOrder, TextRange range) =>
            new LogicalExpression(operand, higherOrder, range);
        [DebuggerStepThrough]
        public static LogicalExpression Create(
            IExpression operand, TextRange range) =>
            new LogicalExpression(operand, UnspecifiedTerm.Instance, range);
    }
}
