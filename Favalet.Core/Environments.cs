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
using Favalet.Contexts.Unifiers;
using Favalet.Ranges;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Favalet.Expressions.Operators;

namespace Favalet
{
    public interface IEnvironments :
        IScopeContext
    {
        ITopology? LastTopology { get; }
        
        void MutableBind(IBoundVariableTerm symbol, IExpression expression);
    }

    public class Environments :
        ScopeContext, IEnvironments, IPlaceholderProvider
    {
        private UnifyContext? lastContext;
        private int placeholderIndex = -1;
        private readonly bool saveLastTopology;

        [DebuggerStepThrough]
        protected Environments(ITypeCalculator typeCalculator, bool saveLastTopology) :
            base(null, typeCalculator) => 
            this.saveLastTopology = saveLastTopology;
        
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
                    (agg, index) => PlaceholderTerm.Create(index, agg, TextRange.Unknown));  // TODO: range
        }

        [DebuggerStepThrough]
        IExpression IPlaceholderProvider.CreatePlaceholder(
            PlaceholderOrderHints orderHint) =>
            this.CreatePlaceholder(orderHint);

        private IExpression InternalInfer(
            UnifyContext unifyContext,
            ReduceContext context,
            IExpression expression)
        {
            Debug.WriteLine($"Infer[{context.GetHashCode()}:before] :");
            Debug.WriteLine(expression.GetXml());
  
            var rewritable = context.MakeRewritable(expression);
            unifyContext.SetTargetRoot(rewritable);

            Debug.WriteLine($"Infer[{context.GetHashCode()}:rewritable] :");
            Debug.WriteLine(rewritable.GetXml());

            var inferred = context.Infer(rewritable);
            unifyContext.SetTargetRoot(inferred);
            
            Debug.WriteLine($"Infer[{context.GetHashCode()}:inferred] :");
            Debug.WriteLine(inferred.GetXml());

            var fixedup = context.Fixup(inferred);
            //unifyContext.SetTargetRoot(fixedup);

            Debug.WriteLine($"Infer[{context.GetHashCode()}:fixedup] :");
            Debug.WriteLine(fixedup.GetXml());

            return fixedup;
        }

        public IExpression Infer(IExpression expression)
        {
            var unifyContext = UnifyContext.Create(this.TypeCalculator, expression);
            var context = ReduceContext.Create(this, this, unifyContext);

            var inferred = this.InternalInfer(unifyContext, context, expression);

            if (this.saveLastTopology)
            {
                this.lastContext = unifyContext;
            }

            return inferred;
        }

        public IExpression Reduce(IExpression expression)
        {
            var unifyContext = UnifyContext.Create(this.TypeCalculator, expression);
            var context = ReduceContext.Create(this, this, unifyContext);

            var inferred = this.InternalInfer(unifyContext, context, expression);
            var reduced = context.Reduce(inferred);
            
            Debug.WriteLine($"Reduce[{context.GetHashCode()}:reduced] :");
            Debug.WriteLine(reduced.GetXml());

            if (this.saveLastTopology)
            {
                this.lastContext = unifyContext;
            }

            return reduced;
        }

        [DebuggerStepThrough]
        public void MutableBind(IBoundVariableTerm symbol, IExpression expression) =>
            base.MutableBind(symbol, expression, true);
        [DebuggerStepThrough]
        internal void UnsafeMutableBind(IBoundVariableTerm symbol, IExpression expression) =>
            base.MutableBind(symbol, expression, false);

        [DebuggerStepThrough]
        public static Environments Create(
#if DEBUG
            bool saveLastTopology = true
#else
            bool saveLastTopology = false
#endif
        )
        {
            var environments =
                new Environments(Favalet.TypeCalculator.Instance, saveLastTopology);
            environments.MutableBindDefaults();
            return environments;
        }
    }

    public static class EnvironmentsExtension
    {
        [DebuggerStepThrough]
        public static void MutableBindDefaults(
            this IEnvironments environments)
        {
            // Type kind symbol.
            environments.MutableBind(
                Generator.kind.Symbol, TextRange.Internal, Generator.kind);
            
            // Lambda operator.
            environments.MutableBind(
                "->", TextRange.Internal, LambdaOperatorExpression.Instance);
        }
        
        [DebuggerStepThrough]
        public static void MutableBind(
            this IEnvironments environment,
            string symbol,
            IExpression expression) =>
            environment.MutableBind(BoundVariableTerm.Create(symbol, TextRange.Unknown), expression);
        
        [DebuggerStepThrough]
        public static void MutableBind(
            this IEnvironments environment,
            string symbol,
            TextRange range,
            IExpression expression) =>
            environment.MutableBind(BoundVariableTerm.Create(symbol, range), expression);
    }
}
