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
using Favalet.Expressions.Operators;
using Favalet.Expressions.Specialized;
using Favalet.Contexts.Unifiers;
using Favalet.Ranges;
using Favalet.Internal;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Favalet
{
    [Flags]
    public enum BoundAttributes
    {
        PrefixLeftToRight = 0x00,
        PrefixRightToLeft = 0x02,
        InfixLeftToRight = 0x01,
        InfixRightToLeft = 0x03,

        InfixMask = 0x01,
        RightToLeftMask = 0x02,
    }
    
    public interface IEnvironments :
        IScopeContext
    {
        ITopology? LastTopology { get; }
        
        void MutableBind(BoundAttributes attributes, IBoundVariableTerm symbol, IExpression expression);
    }

    public class Environments :
        ScopeContext, IEnvironments, IPlaceholderProvider
    {
        private UnifyContext? lastContext;
        private int placeholderIndex = -1;
        private readonly bool saveLastTopology;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerStepThrough]
        public Environments(
            ITypeCalculator typeCalculator, bool saveLastTopology) :
            base(null)
        {
            this.TypeCalculator = typeCalculator;            
            this.saveLastTopology = saveLastTopology;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerStepThrough]
        public Environments(bool saveLastTopology) :
            this(Favalet.TypeCalculator.Instance, saveLastTopology)
        {
        }
        
        public override ITypeCalculator TypeCalculator { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public ITopology? LastTopology =>
            this.lastContext;

        protected virtual void OnReset() =>
            this.MutableBindDefaults();
        
        public void Reset()
        {
            this.CopyInRegistry(null, true);
            this.placeholderIndex = -1;
        }

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
                Memoize();
            
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

            var transposed = context.Transpose(expression);
            unifyContext.SetTargetRoot(transposed);
  
            Debug.WriteLine($"Infer[{context.GetHashCode()}:transposed] :");
            Debug.WriteLine(transposed.GetXml());

            var rewritable = context.MakeRewritable(transposed);
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
        public void MutableBind(
            BoundAttributes attributes,
            IBoundVariableTerm symbol,
            IExpression expression) =>
            base.MutableBind(attributes, symbol, expression, false);

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

    [DebuggerStepThrough]
    public static class EnvironmentsExtension
    {
        public static void MutableBind(
            this IEnvironments environments,
            BoundAttributes attributes,
            string symbol,
            IExpression expression) =>
            environments.MutableBind(
                attributes,
                BoundVariableTerm.Create(symbol, TextRange.Unknown),
                expression);
        
        public static void MutableBind(
            this IEnvironments environments,
            BoundAttributes attributes,
            string symbol,
            TextRange range,
            IExpression expression) =>
            environments.MutableBind(
                attributes,
                BoundVariableTerm.Create(symbol, range),
                expression);
        
        public static void MutableBind(
            this IEnvironments environments,
            IBoundVariableTerm symbol,
            IExpression expression) =>
            environments.MutableBind(
                BoundAttributes.PrefixLeftToRight,
                symbol,
                expression);
        
        public static void MutableBind(
            this IEnvironments environment,
            string symbol,
            IExpression expression) =>
            environment.MutableBind(
                BoundVariableTerm.Create(symbol, TextRange.Unknown),
                expression);
        
        public static void MutableBind(
            this IEnvironments environment,
            string symbol,
            TextRange range,
            IExpression expression) =>
            environment.MutableBind(
                BoundVariableTerm.Create(symbol, range),
                expression);
         
        public static void MutableBindDefaults(
            this IEnvironments environments)
        {
            // Unspecified symbol.
            environments.MutableBind(
                BoundAttributes.PrefixLeftToRight, "_",
                TextRange.Internal, UnspecifiedTerm.Instance);

            // Type fourth symbol.
            environments.MutableBind(
                BoundAttributes.PrefixLeftToRight, "#",
                TextRange.Internal, FourthTerm.Instance);

            // Type kind symbol.
            environments.MutableBind(
                BoundAttributes.PrefixLeftToRight, "*",
                TextRange.Internal, TypeKindTerm.Instance);

            // Lambda operator.
            environments.MutableBind(
                BoundAttributes.InfixMask | BoundAttributes.RightToLeftMask, "->",
                TextRange.Internal, LambdaOperatorExpression.Instance);
        }
    }
}
