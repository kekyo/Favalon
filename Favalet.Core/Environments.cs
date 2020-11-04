using Favalet.Contexts;
using Favalet.Expressions;
using Favalet.Expressions.Specialized;
using Favalet.Contexts.Unifiers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Favalet
{
    public interface IEnvironments :
        IScopeContext
    {
        ITopology? LastTopology { get; }
        
        void MutableBind(IBoundVariableTerm symbol, IExpression expression);
    }

    public sealed class Environments :
        ScopeContext, IEnvironments, IPlaceholderProvider
    {
        private ReduceContext? lastContext;
        private int placeholderIndex = -1;
        private bool saveLastTopology;

        [DebuggerStepThrough]
        private Environments(ITypeCalculator typeCalculator, bool saveLastTopology) :
            base(null, typeCalculator)
        {
            this.saveLastTopology = saveLastTopology;
            this.MutableBind(Generator.kind.Symbol, Generator.kind);
        }
        
        public ITopology? LastTopology =>
            this.lastContext;

        [DebuggerStepThrough]
        internal IExpression CreatePlaceholder(
            PlaceholderOrderHints orderHint)
        {
            var oh = Math.Min(
                (int)PlaceholderOrderHints.DeadEnd,
                Math.Max(0, (int)orderHint));
            var count = Math.Min(
                (int)PlaceholderOrderHints.DeadEnd - oh,
                (int)PlaceholderOrderHints.Fourth);
            
            var indexList =
                Enumerable.Range(0, count).
                Select(_ => Interlocked.Increment(ref this.placeholderIndex)).
                ToArray();
            
            return indexList.
                Reverse().
                Aggregate(
                    (IExpression)DeadEndTerm.Instance,
                    (agg, index) => PlaceholderTerm.Create(index, agg));
        }

        [DebuggerStepThrough]
        IExpression IPlaceholderProvider.CreatePlaceholder(
            PlaceholderOrderHints orderHint) =>
            this.CreatePlaceholder(orderHint);

        private IExpression InternalInfer(
            ReduceContext context,
            IExpression expression)
        {
            Debug.WriteLine($"Infer[{context.GetHashCode()}:before] :");
            Debug.WriteLine(expression.GetXml());
  
            var rewritable = context.MakeRewritable(expression);
            context.SetTargetRoot(rewritable);

#if DEBUG
            Debug.WriteLine($"Infer[{context.GetHashCode()}:rewritable] :");
            Debug.WriteLine(rewritable.GetXml());
#endif            

            var inferred = context.Infer(rewritable);
            context.SetTargetRoot(inferred);
            
#if DEBUG
            Debug.WriteLine($"Infer[{context.GetHashCode()}:inferred] :");
            Debug.WriteLine(inferred.GetXml());
#endif

            context.NormalizeAliases();

            var fixedup = context.Fixup(inferred);
            context.SetTargetRoot(fixedup);

#if DEBUG
            Debug.WriteLine($"Infer[{context.GetHashCode()}:fixedup] :");
            Debug.WriteLine(fixedup.GetXml());
#endif

            return fixedup;
        }

        public IExpression Infer(IExpression expression)
        {
            var unifier = Unifier.Create(this.TypeCalculator, expression);
            var context = new ReduceContext(this, this, unifier);

            var inferred = this.InternalInfer(context, expression);

            if (this.saveLastTopology)
            {
                this.lastContext = context;
            }

            return inferred;
        }

        public IExpression Reduce(IExpression expression)
        {
            var unifier = Unifier.Create(this.TypeCalculator, expression);
            var context = new ReduceContext(this, this, unifier);

            var inferred = this.InternalInfer(context, expression);
            var reduced = context.Reduce(inferred);
            
#if DEBUG
            Debug.WriteLine($"Reduce[{context.GetHashCode()}:reduced] :");
            Debug.WriteLine(reduced.GetXml());
#endif
            if (this.saveLastTopology)
            {
                this.lastContext = context;
            }

            return reduced;
        }

        [DebuggerStepThrough]
        public new void MutableBind(IBoundVariableTerm symbol, IExpression expression) =>
            base.MutableBind(symbol, expression);

        [DebuggerStepThrough]
        public static Environments Create(
            ITypeCalculator typeCalculator,
#if DEBUG
            bool saveLastTopology = true
#else
            bool saveLastTopology = false
#endif
            ) =>
            new Environments(typeCalculator, saveLastTopology);
    }

    public static class EnvironmentExtension
    {
        [DebuggerStepThrough]
        public static void MutableBind(
            this IEnvironments environment,
            string symbol,
            IExpression expression) =>
            environment.MutableBind(BoundVariableTerm.Create(symbol), expression);
    }
}
