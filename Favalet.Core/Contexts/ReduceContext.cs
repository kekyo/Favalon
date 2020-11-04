using Favalet.Contexts.Unifiers;
using Favalet.Expressions;
using Favalet.Expressions.Specialized;
using Favalet.Internal;
using System.Diagnostics;

namespace Favalet.Contexts
{
    public interface IMakeRewritableContext
    {
        IExpression MakeRewritable(IExpression expression);
        IExpression MakeRewritableHigherOrder(IExpression higherOrder);
    }
    
    public interface IUnsafePlaceholderResolver
    {
        IExpression? UnsafeResolve(IPlaceholderTerm placeholder);
    }

    public static class UnsafePlaceholderResolverExtension
    {
        public static IExpression UnsafeResolveWhile(this IUnsafePlaceholderResolver resolver, IExpression expression)
        {
            var current = expression;
            while (true)
            {
                switch (current)
                {
                    case IPlaceholderTerm placeholder:
                        if (resolver.UnsafeResolve(placeholder) is IExpression resolved)
                        {
                            current = resolved;
                            continue;
                        }
                        else
                        {
                            return current;
                        }
                    
                    case IFunctionExpression(IExpression parameter, IExpression result):
                        return FunctionExpression.Create(
                            UnsafeResolveWhile(resolver, parameter),
                            UnsafeResolveWhile(resolver, result));
                    
                    default:
                        return current;
                }
            }
        }
    }

    public interface IInferContext :
        IScopeContext, IMakeRewritableContext, IUnsafePlaceholderResolver
    {
        IExpression Infer(IExpression expression);
    
        IInferContext Bind(IBoundVariableTerm parameter, IExpression expression);

        void Unify(
            IExpression fromHigherOrder,
            IExpression toHigherOrder,
            bool bidirectional);
    }

    public interface IFixupContext :
        IScopeContext
    {
        IExpression Fixup(IExpression expression);
        IExpression FixupHigherOrder(IExpression higherOrder);
        
        IExpression? Resolve(IPlaceholderTerm placeholder);
    }

    public interface IReduceContext :
        IScopeContext
    {
        IExpression Reduce(IExpression expression);
    
        IReduceContext Bind(IBoundVariableTerm parameter, IExpression expression);
    }

    internal abstract class FixupContext :
        IFixupContext, IUnsafePlaceholderResolver
    {
        [DebuggerStepThrough]
        protected FixupContext(ITypeCalculator typeCalculator) =>
            this.TypeCalculator = typeCalculator;

        public ITypeCalculator TypeCalculator { get; }

        [DebuggerStepThrough]
        public IExpression Fixup(IExpression expression) =>
            expression is Expression expr ? expr.InternalFixup(this) : expression;

        [DebuggerStepThrough]
        public IExpression FixupHigherOrder(IExpression higherOrder)
        {
            var fixedup = higherOrder is Expression expr ?
                expr.InternalFixup(this) :
                higherOrder;

            return this.TypeCalculator.Compute(fixedup);
        }

        public abstract IExpression? Resolve(IPlaceholderTerm placeholder);

        [DebuggerStepThrough]
        IExpression? IUnsafePlaceholderResolver.UnsafeResolve(IPlaceholderTerm placeholder) =>
            this.Resolve(placeholder);

        public virtual VariableInformation[] LookupVariables(string symbol) =>
            ArrayEx.Empty<VariableInformation>();
    }

    internal sealed class ReduceContext :
        FixupContext, IInferContext, IReduceContext, ITopology
    {
        private readonly Environments rootScope;
        private readonly IScopeContext parentScope;
        private readonly Unifier unifier;
        private IBoundVariableTerm? boundSymbol;
        private IExpression? boundExpression;
        private PlaceholderOrderHints orderHint = PlaceholderOrderHints.VariableOrAbove;

        [DebuggerStepThrough]
        public ReduceContext(
            Environments rootScope,
            IScopeContext parentScope,
            Unifier unifier) :
            base(rootScope.TypeCalculator)
        {
            this.rootScope = rootScope;
            this.parentScope = parentScope;
            this.unifier = unifier;
        }

        [DebuggerStepThrough]
        public void SetTargetRoot(IExpression targetRoot) =>
            this.unifier.SetTargetRoot(targetRoot);

        public IExpression MakeRewritable(IExpression expression)
        {
            if (this.orderHint >= PlaceholderOrderHints.DeadEnd)
            {
                return DeadEndTerm.Instance;
            }
            
            var rewritable = expression is Expression expr ?
                expr.InternalMakeRewritable(this) :
                expression;

            // Cannot replace these terms.
            if (rewritable is IPlaceholderTerm ||
                rewritable is IVariableTerm ||
                rewritable is DeadEndTerm ||
                rewritable is FourthTerm)
            {
                return rewritable;
            }

            // The unspecified term always turns to placeholder term.
            if (rewritable is UnspecifiedTerm)
            {
                return this.rootScope.CreatePlaceholder(this.orderHint);
            }

            return rewritable;
        }
            
        public IExpression MakeRewritableHigherOrder(IExpression higherOrder)
        {
            this.orderHint++;
            
            var rewritable = this.MakeRewritable(higherOrder);
            
            this.orderHint--;
            Debug.Assert(this.orderHint >= PlaceholderOrderHints.VariableOrAbove);
            
            return rewritable;
        }

        [DebuggerStepThrough]
        public void NormalizeAliases() =>
            this.unifier.NormalizeAliases();

        [DebuggerStepThrough]
        public IExpression Infer(IExpression expression) =>
            expression is Expression expr ? expr.InternalInfer(this) : expression;
        [DebuggerStepThrough]
        public IExpression Reduce(IExpression expression) =>
            expression is Expression expr ? expr.InternalReduce(this) : expression;

        private ReduceContext Bind(
            IBoundVariableTerm symbol, IExpression expression)
        {
            var newContext = new ReduceContext(
                this.rootScope,
                this,
                this.unifier);

            newContext.boundSymbol = symbol;
            newContext.boundExpression = expression;

            return newContext;
        }

        [DebuggerStepThrough]
        IInferContext IInferContext.Bind(
            IBoundVariableTerm symbol, IExpression expression) =>
            this.Bind(symbol, expression);
        [DebuggerStepThrough]
        IReduceContext IReduceContext.Bind(
            IBoundVariableTerm symbol, IExpression expression) =>
            this.Bind(symbol, expression);

        [DebuggerStepThrough]
        public void Unify(
            IExpression fromHigherOrder,
            IExpression toHigherOrder,
            bool bidirectional = false) =>
            this.unifier.Unify(fromHigherOrder, toHigherOrder, bidirectional);

        [DebuggerStepThrough]
        public override IExpression? Resolve(IPlaceholderTerm placeholder) =>
            this.unifier.Resolve(placeholder);

        public override VariableInformation[] LookupVariables(string symbol) =>
            // TODO: improving when identity's higher order acceptable
            // TODO: what acceptable (narrowing, widening)
            this.boundSymbol is IBoundVariableTerm p &&
            boundExpression is IExpression expr &&
            p.Symbol.Equals(symbol) ?
                new[] { VariableInformation.Create(symbol, p.HigherOrder, expr) } :
                parentScope.LookupVariables(symbol);

        public string View
        {
            [DebuggerStepThrough]
            get => this.unifier.View;
        }

        public string Dot
        {
            [DebuggerStepThrough]
            get => this.unifier.Dot;
        }

        [DebuggerStepThrough]
        public override string ToString() =>
            "ReduceContext: " + this.View;
    }
}
