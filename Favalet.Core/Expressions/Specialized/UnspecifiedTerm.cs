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
using System.Linq;
using Favalet.Ranges;

namespace Favalet.Expressions.Specialized
{
    [DebuggerStepThrough]
    public sealed class UnspecifiedTerm :
        Expression, IIgnoreUnificationTerm
    {
        private UnspecifiedTerm(TextRange range) :
            base(range)
        {
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public override IExpression HigherOrder =>
            DeadEndTerm.Instance;

        public bool Equals(UnspecifiedTerm rhs) =>
            rhs != null;

        public override bool Equals(IExpression? other) =>
            other is UnspecifiedTerm;

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

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            Enumerable.Empty<object>();

        protected override string GetPrettyString(IPrettyStringContext context) =>
            "_";

        public static readonly UnspecifiedTerm Instance =
            new UnspecifiedTerm(TextRange.Unknown);   // TODO: range
    }
}
