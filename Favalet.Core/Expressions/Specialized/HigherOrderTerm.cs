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
    public abstract class HigherOrderTerm<TTerm> :
        Expression, ITerm
        where TTerm : ITerm
    {
        private protected HigherOrderTerm(TextRange range) :
            base(range)
        { }

        public bool Equals(TTerm? rhs) =>
            rhs != null;

        public override bool Equals(IExpression? other) =>
            other is TTerm;

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
    }
        
    [DebuggerStepThrough]
    public sealed class TypeKindTerm :
        HigherOrderTerm<TypeKindTerm>
    {
        private TypeKindTerm() :
            base(TextRange.Internal)
        { }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public override IExpression HigherOrder =>
            FourthTerm.Instance;

        protected override string GetPrettyString(IPrettyStringContext context) =>
            "*";

        public static readonly TypeKindTerm Instance =
            new TypeKindTerm();
    }

    [DebuggerStepThrough]
    public sealed class FourthTerm :
        HigherOrderTerm<FourthTerm>
    {
        private FourthTerm() :
            base(TextRange.Internal)
        { }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public override IExpression HigherOrder =>
            DeadEndTerm.Instance;

        protected override string GetPrettyString(IPrettyStringContext context) =>
            "#";

        public static readonly FourthTerm Instance =
            new FourthTerm();
    }
}
