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
using Favalet.Expressions;
using Favalet.Expressions.Specialized;
using Favalet.Ranges;
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Xml.Linq;

namespace Favalet
{
    // For testing purpose.
    internal sealed class PseudoPlaceholderProvider
    {
        private volatile int index = -1;

        private PseudoPlaceholderProvider()
        { }

        [DebuggerStepThrough]
        public PseudoPlaceholderTerm CreatePlaceholder() =>
            new PseudoPlaceholderTerm(Interlocked.Increment(ref this.index));

        [DebuggerStepThrough]
        public static PseudoPlaceholderProvider Create() =>
            new PseudoPlaceholderProvider();

        internal sealed class PseudoPlaceholderTerm :
            Expression, ITerm
        {
            public readonly int Index;

            public PseudoPlaceholderTerm(int index) :
                base(TextRange.Unknown) =>
                this.Index = index;

            public override IExpression HigherOrder =>
                UnspecifiedTerm.Instance;   // Stop traversal by ignoring marker.

            public string Symbol =>
                $"'{this.Index}";

            protected override IExpression Transpose(ITransposeContext context) =>
                throw new NotImplementedException();

            protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
                throw new NotImplementedException();

            protected override IExpression Fixup(IFixupContext context) =>
                throw new NotImplementedException();

            protected override IExpression Infer(IInferContext context) =>
                throw new NotImplementedException();

            protected override IExpression Reduce(IReduceContext context) =>
                throw new NotImplementedException();

            public override int GetHashCode() =>
                this.Index.GetHashCode();

            public bool Equals(PseudoPlaceholderTerm rhs) =>
                false;

            public override bool Equals(IExpression? other) =>
                false;

            protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
                new[] { new XAttribute("index", this.Index) };

            protected override string GetPrettyString(IPrettyStringContext context) =>
                this.Symbol;
        }
    }
}
