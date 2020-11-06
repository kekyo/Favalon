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
using System;
using System.Collections;
using System.Diagnostics;
using System.Xml.Linq;
using Favalet.Ranges;

namespace Favalet.Expressions
{
    public interface IExpression : IEquatable<IExpression?>
    {
        string Type { get; }

        IExpression HigherOrder { get; }
        
        TextRange Range { get; }
        
        bool IsContainsPlaceholder { get; }
    }

    public interface ITerm : IExpression
    {
    }

    #pragma warning disable CS0659

    [DebuggerDisplay("{Readable}")]
    public abstract class Expression :
        IExpression
    {
        public readonly TextRange Range;

        [DebuggerStepThrough]
        protected Expression(TextRange range) =>
            this.Range = range;

        public string Type =>
            this.GetType().Name.
            Replace("Expression", string.Empty).
            Replace("Term", string.Empty);

        public abstract IExpression HigherOrder { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        TextRange IExpression.Range =>
            this.Range;

        public bool IsContainsPlaceholder
        {
            [DebuggerStepThrough]
            get => this switch
                {
                   IPlaceholderTerm placeholder => true,
                   IPairExpression pair =>
                       pair.Left.IsContainsPlaceholder || pair.Right.IsContainsPlaceholder,
                   _ => false
                } ||
                this.HigherOrder switch
                {
                   DeadEndTerm _ => false,
                   FourthTerm _ => false,
                   _ => this.HigherOrder.IsContainsPlaceholder
                };
        }

        protected abstract IExpression MakeRewritable(IMakeRewritableContext context);
        protected abstract IExpression Infer(IInferContext context);
        protected abstract IExpression Fixup(IFixupContext context);
        protected abstract IExpression Reduce(IReduceContext context);

        [DebuggerStepThrough]
        internal IExpression InternalMakeRewritable(IMakeRewritableContext context) =>
            this.MakeRewritable(context);
        [DebuggerStepThrough]
        internal IExpression InternalInfer(IInferContext context) =>
            this.Infer(context);
        [DebuggerStepThrough]
        internal IExpression InternalFixup(IFixupContext context) =>
            this.Fixup(context);
        [DebuggerStepThrough]
        internal IExpression InternalReduce(IReduceContext context) =>
            this.Reduce(context);

        protected abstract IEnumerable GetXmlValues(IXmlRenderContext context);

        protected abstract string GetPrettyString(IPrettyStringContext context);

        [DebuggerStepThrough]
        internal IEnumerable InternalGetXmlValues(IXmlRenderContext context) =>
            this.GetXmlValues(context);
        [DebuggerStepThrough]
        internal string InternalGetPrettyString(IPrettyStringContext context) =>
            this.GetPrettyString(context);

        public string Xml =>
            this.GetXml().ToString();
        public string Strict =>
            $"{this.Type}: {this.GetPrettyString(PrettyStringTypes.Strict)}";
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string Readable =>
            $"{this.Type}: {this.GetPrettyString(PrettyStringTypes.Readable)}";

        public sealed override string ToString() =>
            this.Readable;

        public abstract bool Equals(IExpression? other);

        [DebuggerStepThrough]
        public override bool Equals(object obj) =>
            this.Equals(obj as IExpression);
    }

    public static class ExpressionExtension
    {
        public static bool ExactEquals(this IExpression lhs, IExpression rhs) =>
            object.ReferenceEquals(lhs, rhs) ||
            (lhs, rhs) switch
            {
                (FourthTerm _, FourthTerm _) => true,
                (FourthTerm _, _) => false,
                (_, FourthTerm _) => false,
                _ =>
                    lhs.Equals(rhs) &&
                    ExactEquals(lhs.HigherOrder, rhs.HigherOrder)
            };

        [DebuggerStepThrough]
        public static XElement GetXml(this IExpression expression) =>
            XmlRenderContext.Create().
            GetXml(expression);

        [DebuggerStepThrough]
        public static string GetPrettyString(
            this IExpression expression,
            PrettyStringTypes type) =>
            PrettyStringContext.Create(type).
            GetPrettyString(expression);
    }
}
