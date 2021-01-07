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

using Favalet.Contexts.Unifiers;
using Favalet.Expressions;
using Favalet.Expressions.Specialized;
using System.Diagnostics;

namespace Favalet.Contexts
{
    public interface ITransposeContext :
        IScopeContext
    {
        IExpression Transpose(IExpression expression);
    }
    
    public interface IMakeRewritableContext
    {
        IExpression MakeRewritable(IExpression expression);
        IExpression MakeRewritableHigherOrder(IExpression higherOrder);
    }

    public interface IInferContext :
        IScopeContext, IMakeRewritableContext
    {
        IExpression Infer(IExpression expression);
    
        IInferContext Bind(
            BoundAttributes attributes,
            IBoundVariableTerm parameter,
            IExpression expression);

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
    
        IReduceContext Bind(
            BoundAttributes attributes,
            IBoundVariableTerm parameter,
            IExpression expression);
    }

    internal sealed class ReduceContext :
        ScopeContext, ITransposeContext, IInferContext, IFixupContext, IReduceContext
    {
        private readonly Environments rootScope;
        private readonly UnifyContext unifyContext;
        private PlaceholderOrderHints orderHint = PlaceholderOrderHints.VariableOrAbove;

        [DebuggerStepThrough]
        private ReduceContext(
            Environments rootScope,
            ScopeContext parentScope,
            UnifyContext unifyContext) :
            base(parentScope)
        {
            this.rootScope = rootScope;
            this.unifyContext = unifyContext;
        }

        public override ITypeCalculator TypeCalculator
        {
            [DebuggerStepThrough]
            get => this.rootScope.TypeCalculator;
        }

        [DebuggerStepThrough]
        public IExpression Transpose(IExpression expression) =>
            expression is Expression expr ? expr.InternalTranspose(this) : expression;

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
        public IExpression Infer(IExpression expression) =>
            expression is Expression expr ? expr.InternalInfer(this) : expression;

        [DebuggerStepThrough]
        public IExpression Fixup(IExpression expression) =>
            expression is Expression expr ? expr.InternalFixup(this) : expression;
        
        [DebuggerStepThrough]
        public IExpression Reduce(IExpression expression) =>
            expression is Expression expr ? expr.InternalReduce(this) : expression;

        private ReduceContext Bind(
            BoundAttributes attributes,
            IBoundVariableTerm symbol,
            IExpression expression)
        {
            var newContext = new ReduceContext(
                this.rootScope,
                this,
                this.unifyContext);
            
            newContext.MutableBind(
                attributes,
                symbol,
                expression,
                false);

            return newContext;
        }

        [DebuggerStepThrough]
        IInferContext IInferContext.Bind(
            BoundAttributes attributes,
            IBoundVariableTerm symbol,
            IExpression expression) =>
            this.Bind(attributes, symbol, expression);
        [DebuggerStepThrough]
        IReduceContext IReduceContext.Bind(
            BoundAttributes attributes,
            IBoundVariableTerm symbol,
            IExpression expression) =>
            this.Bind(attributes, symbol, expression);

        [DebuggerStepThrough]
        public void Unify(
            IExpression fromHigherOrder,
            IExpression toHigherOrder,
            bool bidirectional = false) =>
            Unifier.Instance.Unify(this.unifyContext, fromHigherOrder, toHigherOrder, bidirectional);
        
        [DebuggerStepThrough]
        public IExpression FixupHigherOrder(IExpression higherOrder)
        {
            var fixedup = higherOrder is Expression expr ?
                expr.InternalFixup(this) :
                higherOrder;
            
            // Reduce higher order.
            //var calculated = this.TypeCalculator.Compute(fixedup);
            //return this.Reduce(calculated);
            return this.Reduce(fixedup);
        }

        [DebuggerStepThrough]
        public IExpression? Resolve(IPlaceholderTerm placeholder) =>
            this.unifyContext.Resolve(placeholder);

        [DebuggerStepThrough]
        public override string ToString() =>
            "ReduceContext: " + this.unifyContext;

        [DebuggerStepThrough]
        public static ReduceContext Create(
            Environments rootScope,
            ScopeContext parentScope,
            UnifyContext unifyContext) =>
            new ReduceContext(rootScope, parentScope, unifyContext);
    }

    public static class ReduceContextExtension
    {
        [DebuggerStepThrough]
        public static IInferContext Bind(
            this IInferContext context, IBoundVariableTerm symbol, IExpression expression) =>
            context.Bind(BoundAttributes.PrefixLeftToRight, symbol, expression);
        
        [DebuggerStepThrough]
        public static IReduceContext Bind(
            this IReduceContext context, IBoundVariableTerm symbol, IExpression expression) =>
            context.Bind(BoundAttributes.PrefixLeftToRight, symbol, expression);
    }
}
