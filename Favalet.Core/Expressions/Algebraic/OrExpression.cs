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
using System.Diagnostics;

namespace Favalet.Expressions.Algebraic
{
    public interface IOrExpression : IBinaryExpression
    {
    }

    public sealed class OrExpression :
        BinaryExpression<IOrExpression>,
        IOrExpression
    {
        [DebuggerStepThrough]
        private OrExpression(
            IExpression left, IExpression right, IExpression higherOrder) :
            base(left, right, higherOrder)
        { }

        [DebuggerStepThrough]
        internal override IExpression OnCreate(
            IExpression left, IExpression right, IExpression higherOrder) =>
            new OrExpression(left, right, higherOrder);

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                $"{context.GetPrettyString(this.Left)} || {context.GetPrettyString(this.Right)}");

        [DebuggerStepThrough]
        public static OrExpression Create(
            IExpression left, IExpression right, IExpression higherOrder) =>
            new OrExpression(left, right, higherOrder);
        [DebuggerStepThrough]
        public static OrExpression Create(
            IExpression left, IExpression right) =>
            new OrExpression(left, right, UnspecifiedTerm.Instance);
    }
}
