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

namespace Favalet.Expressions.Algebraic
{
    [DebuggerStepThrough]
    internal abstract class FlattenedExpression :
        Expression
    {
        public readonly IExpression[] Operands;

        protected FlattenedExpression(IExpression[] operands, TextRange range) :
            base(range) =>
            this.Operands = operands;

        public sealed override int GetHashCode() =>
            this.Operands.Aggregate(0, (agg, operand) => agg ^ operand?.GetHashCode() ?? 0);

        protected sealed override IExpression Transpose(ITransposeContext context) =>
            throw new InvalidOperationException();

        protected sealed override IExpression MakeRewritable(IMakeRewritableContext context) =>
            throw new InvalidOperationException();

        protected sealed override IExpression Infer(IInferContext context) =>
            throw new InvalidOperationException();

        protected sealed override IExpression Fixup(IFixupContext context) =>
            throw new InvalidOperationException();

        protected sealed override IExpression Reduce(IReduceContext context) =>
            throw new InvalidOperationException();

        protected sealed override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            this.Operands.Select(context.GetXml);

        public void Deconstruct(out IExpression[] operands) =>
            operands = this.Operands;

        public static IEnumerable<IExpression> Flatten<TBinaryExpression>(
            IExpression left, IExpression right)
            where TBinaryExpression : IBinaryExpression
        {
            var lf = left is TBinaryExpression lb ?
                Flatten<TBinaryExpression>(lb.Left, lb.Right) :
                new[] { left };

            var rf = right is TBinaryExpression rb ?
                Flatten<TBinaryExpression>(rb.Left, rb.Right) :
                new[] { right };

            return lf.Concat(rf);
        }

        public static IExpression Flatten(
            IExpression expression,
            Func<IEnumerable<IExpression>, IEnumerable<IExpression>> sorter) =>
            expression switch
            {
                IAndExpression and => new AndFlattenedExpression(
                    sorter(Flatten<IAndExpression>(and.Left, and.Right)).Memoize(),
                    and.HigherOrder,
                    TextRange.Unknown),  // TODO: range
                IOrExpression or => new OrFlattenedExpression(
                    sorter(Flatten<IOrExpression>(or.Left, or.Right)).Memoize(),
                    or.HigherOrder,
                    TextRange.Unknown),  // TODO: range
                _ => expression
            };

        public static IEnumerable<IExpression> FlattenAll<TBinaryExpression>(
            IExpression left, IExpression right,
            Func<IEnumerable<IExpression>, IEnumerable<IExpression>> sorter)
            where TBinaryExpression : IBinaryExpression
        {
            var lf = left is TBinaryExpression lb ?
                FlattenAll<TBinaryExpression>(lb.Left, lb.Right, sorter) :
                new[] { FlattenAll(left, sorter) };

            var rf = right is TBinaryExpression rb ?
                FlattenAll<TBinaryExpression>(rb.Left, rb.Right, sorter) :
                new[] { FlattenAll(right, sorter) };

            return lf.Concat(rf);
        }

        public static IExpression FlattenAll(
            IExpression expression,
            Func<IEnumerable<IExpression>, IEnumerable<IExpression>> sorter) =>
            expression switch
            {
                IAndExpression and => new AndFlattenedExpression(
                    sorter(FlattenAll<IAndExpression>(and.Left, and.Right, sorter)).Memoize(),
                    and.HigherOrder,
                    TextRange.Unknown),  // TODO: range
                IOrExpression or => new OrFlattenedExpression(
                    sorter(FlattenAll<IOrExpression>(or.Left, or.Right, sorter)).Memoize(),
                    or.HigherOrder,
                    TextRange.Unknown),  // TODO: range
                _ => expression
            };
    }

    [DebuggerStepThrough]
    internal sealed class AndFlattenedExpression : FlattenedExpression
    {
        public AndFlattenedExpression(
            IExpression[] operands, IExpression higherOrder, TextRange range) :
            base(operands, range) =>
            this.HigherOrder = higherOrder;

        public override IExpression HigherOrder { get; }

        public override bool Equals(IExpression? other) =>
            other is AndFlattenedExpression rhs &&
            this.Operands.EqualsPartiallyOrdered(rhs.Operands);

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                StringUtilities.Join(
                    " && ",
                    this.Operands.Select(context.GetPrettyString)));
    }

    [DebuggerStepThrough]
    internal sealed class OrFlattenedExpression : FlattenedExpression
    {
        public OrFlattenedExpression(
            IExpression[] operands, IExpression higherOrder, TextRange range) :
            base(operands, range) =>
            this.HigherOrder = higherOrder;

        public override IExpression HigherOrder { get; }

        public override bool Equals(IExpression? other) =>
            other is OrFlattenedExpression rhs &&
            this.Operands.EqualsPartiallyOrdered(rhs.Operands);

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                StringUtilities.Join(
                    " || ",
                    this.Operands.Select(context.GetPrettyString)));
    }
}
