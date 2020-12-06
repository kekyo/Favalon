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

using System.Collections;
using System.Diagnostics;
using System.Xml.Linq;
using Favalet.Contexts;
using Favalet.Ranges;

namespace Favalet.Expressions.Specialized
{
    public enum PlaceholderOrderHints
    {
        VariableOrAbove = 0,
        TypeOrAbove,
        KindOrAbove,
        Fourth,
        DeadEnd
    }

    public interface IPlaceholderProvider
    {
        IExpression CreatePlaceholder(PlaceholderOrderHints orderHint);
    }

    public interface IPlaceholderTerm :
        IIdentityTerm
    {
        int Index { get; }
    }

    public sealed class PlaceholderTerm :
        Expression, IPlaceholderTerm
    {
        public readonly int Index;

        [DebuggerStepThrough]
        private PlaceholderTerm(int index, IExpression higherOrder, TextRange range) :
            base(range)
        {
            this.Index = index;
            this.HigherOrder = higherOrder;
        }

        public override IExpression HigherOrder { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string Symbol
        {
            [DebuggerStepThrough]
            get => $"'{this.Index}";
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object IIdentityTerm.Identity
        {
            [DebuggerStepThrough]
            get => this.Index;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int IPlaceholderTerm.Index
        {
            [DebuggerStepThrough]
            get => this.Index;
        }

        public override int GetHashCode() =>
            this.Index.GetHashCode();

        public bool Equals(IIdentityTerm rhs) =>
            this.Index.Equals(rhs.Identity);

        public override bool Equals(IExpression? other) =>
            other is IIdentityTerm rhs && this.Equals(rhs);

        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            this;  // Placeholder already rewritable on the unifier infrastructure.

        protected override IExpression Infer(IInferContext context) =>
            this;

        protected override IExpression Fixup(IFixupContext context)
        {
            if (context.Resolve(this) is { } resolved &&
                !resolved.Equals(this))
            {
                //Debug.WriteLine(resolved.GetPrettyString(PrettyStringTypes.Readable));
                return context.Fixup(resolved);
            }
            else
            {
                var higherOrder = context.FixupHigherOrder(this.HigherOrder);

                if (object.ReferenceEquals(this.HigherOrder, higherOrder))
                {
                    return this;
                }
                else
                {
                    return new PlaceholderTerm(this.Index, higherOrder, this.Range);
                }
            }
        }

        protected override IExpression Reduce(IReduceContext context) =>
            this;

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            new[] { new XAttribute("index", this.Index) };

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                this.Symbol);

        [DebuggerStepThrough]
        internal static PlaceholderTerm Create(int index, IExpression higherOrder, TextRange range) =>
            new PlaceholderTerm(index, higherOrder, range);
    }

    [DebuggerStepThrough]
    public static class PlaceholderTermExtension
    {
        public static void Deconstruct(
            this IPlaceholderTerm placeholder,
            out int index) =>
            index = placeholder.Index;
    }
}
