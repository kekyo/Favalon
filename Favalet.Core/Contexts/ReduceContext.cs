﻿/////////////////////////////////////////////////////////////////////////////////////////////////
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
using Favalet.Internal;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Favalet.Contexts
{
    public interface IMakeRewritableContext
    {
        IExpression MakeRewritable(IExpression expression);
        IExpression MakeRewritableHigherOrder(IExpression higherOrder);
    }

    public interface IInferContext :
        IScopeContext, IMakeRewritableContext
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

    internal sealed class ReduceContext :
        IInferContext, IFixupContext, IReduceContext
    {
        private readonly Environments rootScope;
        private readonly IScopeContext parentScope;
        private readonly UnifyContext unifyContext;
        private VariableInformationRegistry? registry;
        private PlaceholderOrderHints orderHint = PlaceholderOrderHints.VariableOrAbove;

        [DebuggerStepThrough]
        private ReduceContext(
            Environments rootScope,
            IScopeContext parentScope,
            UnifyContext unifyContext)
        {
            this.rootScope = rootScope;
            this.parentScope = parentScope;
            this.unifyContext = unifyContext;
        }

        public ITypeCalculator TypeCalculator
        {
            [DebuggerStepThrough]
            get => this.rootScope.TypeCalculator;
        }

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

        [DebuggerStepThrough]
        private ReduceContext Bind(
            IBoundVariableTerm symbol, IExpression expression)
        {
            var newContext = new ReduceContext(
                this.rootScope,
                this,
                this.unifyContext);

            newContext.registry = new VariableInformationRegistry();
            newContext.registry.Register(symbol, expression, true);

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
        public IEnumerable<VariableInformation> LookupVariables(string symbol)
        {
            var overrideVariables =
                this.registry?.Lookup(symbol) ??
                ArrayEx.Empty<VariableInformation>();
            return (overrideVariables.Length >= 1) ?
                overrideVariables :
                this.parentScope?.LookupVariables(symbol) ??
                    Enumerable.Empty<VariableInformation>();
        }

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
            this.unifyContext.TryResolveRecursive(placeholder, out var resolved) ? resolved : null;

        [DebuggerStepThrough]
        public override string ToString() =>
            "ReduceContext: " + this.unifyContext;

        [DebuggerStepThrough]
        public static ReduceContext Create(
            Environments rootScope,
            IScopeContext parentScope,
            UnifyContext unifyContext) =>
            new ReduceContext(rootScope, parentScope, unifyContext);
    }
}
