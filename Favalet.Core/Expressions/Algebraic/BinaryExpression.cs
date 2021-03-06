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

using System;
using Favalet.Contexts;
using System.Collections;
using System.Diagnostics;
using Favalet.Expressions.Specialized;
using Favalet.Ranges;

namespace Favalet.Expressions.Algebraic
{
    public interface IBinaryExpression : IExpression
    {
        IExpression Left { get; }
        IExpression Right { get; }
    }

    public abstract class BinaryExpression<TBinaryExpression> :
        Expression, IBinaryExpression, IPairExpression
        where TBinaryExpression : IBinaryExpression
    {
        public readonly IExpression Left;
        public readonly IExpression Right;

        [DebuggerStepThrough]
        protected BinaryExpression(
            IExpression left, IExpression right, IExpression higherOrder, TextRange range) :
            base(range)
        {
            this.Left = left;
            this.Right = right;
            this.HigherOrder = higherOrder;
        }

        public sealed override IExpression HigherOrder { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IExpression IBinaryExpression.Left
        {
            [DebuggerStepThrough]
            get => this.Left;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IExpression IBinaryExpression.Right
        {
            [DebuggerStepThrough]
            get => this.Right;
        }
        
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IExpression IPairExpression.Left
        {
            [DebuggerStepThrough]
            get => this.Left;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IExpression IPairExpression.Right
        {
            [DebuggerStepThrough]
            get => this.Right;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Type IPairExpression.IdentityType
        {
            [DebuggerStepThrough]
            get => typeof(TBinaryExpression);
        }

        [DebuggerStepThrough]
        IExpression IPairExpression.Create(
            IExpression left, IExpression right, TextRange range) =>
            this.OnCreate(left, right, UnspecifiedTerm.Instance, range);
        
        internal abstract IExpression OnCreate(
            IExpression left, IExpression right, IExpression higherOrder, TextRange range);

        protected sealed override IExpression Transpose(ITransposeContext context) =>
            this.OnCreate(
                context.Transpose(this.Left),
                context.Transpose(this.Right),
                context.Transpose(this.HigherOrder),
                this.Range);

        protected sealed override IExpression MakeRewritable(IMakeRewritableContext context) =>
            this.OnCreate(
                context.MakeRewritable(this.Left),
                context.MakeRewritable(this.Right),
                context.MakeRewritableHigherOrder(this.HigherOrder),
                this.Range);

        protected sealed override IExpression Infer(IInferContext context)
        {
            var left = context.Infer(this.Left);
            var right = context.Infer(this.Right);
            var higherOrder = context.Infer(this.HigherOrder);

            context.Unify(left.HigherOrder, right.HigherOrder, true);
            
            context.Unify(left.HigherOrder, higherOrder, false);
            context.Unify(right.HigherOrder, higherOrder, false);

            if (object.ReferenceEquals(this.Left, left) &&
                object.ReferenceEquals(this.Right, right) &&
                object.ReferenceEquals(this.HigherOrder, higherOrder))
            {
                return this;
            }
            else
            {
                return this.OnCreate(left, right, higherOrder, this.Range);
            }
        }

        protected sealed override IExpression Fixup(IFixupContext context)
        {
            var left = context.Fixup(this.Left);
            var right = context.Fixup(this.Right);
            var higherOrder = context.FixupHigherOrder(this.HigherOrder);

            if (object.ReferenceEquals(this.Left, left) &&
                object.ReferenceEquals(this.Right, right) &&
                object.ReferenceEquals(this.HigherOrder, higherOrder))
            {
                return this;
            }
            else
            {
                return this.OnCreate(left, right, higherOrder, this.Range);
            }
        }

        protected sealed override IExpression Reduce(IReduceContext context)
        {
            var left = context.Reduce(this.Left);
            var right = context.Reduce(this.Right);

            if (object.ReferenceEquals(this.Left, left) &&
                object.ReferenceEquals(this.Right, right))
            {
                return this;
            }
            else
            {
                return this.OnCreate(left, right, this.HigherOrder, this.Range);
            }
        }

        public override int GetHashCode() =>
            this.Left.GetHashCode() ^ this.Right.GetHashCode();

        public bool Equals(TBinaryExpression rhs) =>
            this.Left.Equals(rhs.Left) && this.Right.Equals(rhs.Right);

        public override bool Equals(IExpression? other) =>
            other is TBinaryExpression rhs && this.Equals(rhs);

        [DebuggerStepThrough]
        protected sealed override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            new[] { context.GetXml(this.Left), context.GetXml(this.Right) };

        [DebuggerStepThrough]
        protected sealed override string GetPrettyString(IPrettyStringContext context) =>
            context.GetPrettyString(FlattenedExpression.FlattenAll(this, e => e));
    }

    public static class BinaryExpressionExtension
    {
        [DebuggerStepThrough]
        public static void Deconstruct(
            this IBinaryExpression binary,
            out IExpression left,
            out IExpression right)
        {
            left = binary.Left;
            right = binary.Right;
        }
    }
}
