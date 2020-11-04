using Favalet.Contexts;
using System.Collections;
using System.Diagnostics;
using System.Xml.Linq;

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
        private PlaceholderTerm(int index, IExpression higherOrder)
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
            if (context.Resolve(this) is IExpression resolved &&
                !this.Equals(resolved))
            {
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
                    return new PlaceholderTerm(this.Index, higherOrder);
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
        internal static PlaceholderTerm Create(int index, IExpression higherOrder) =>
            new PlaceholderTerm(index, higherOrder);
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
