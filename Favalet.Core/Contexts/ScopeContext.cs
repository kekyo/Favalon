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

using System;
using System.ComponentModel;
using Favalet.Expressions;
using Favalet.Expressions.Specialized;
using Favalet.Internal;
using System.Diagnostics;

namespace Favalet.Contexts
{
    [DebuggerStepThrough]
    public readonly struct BoundVariables
    {
        public readonly BoundAttributes Attributes;
        public readonly VariableInformation[] Variables;

        private BoundVariables(BoundAttributes attributes, VariableInformation[] vis)
        {
            this.Attributes = attributes;
            this.Variables = vis;
        }

        public void Deconstruct(
            out BoundAttributes attributes,
            out VariableInformation[] vis)
        {
            attributes = this.Attributes;
            vis = this.Variables;
        }
        
        public static BoundVariables Create(BoundAttributes attributes, VariableInformation[] vis) =>
            new BoundVariables(attributes, vis);
    }
    
    public interface IScopeContext
    {
        ITypeCalculator TypeCalculator { get; }

        BoundVariables? LookupVariables(string symbol);
    }

    public abstract class ScopeContext :
        IScopeContext
    {
        private readonly ScopeContext? parentScope;
        private IVariableInformationRegistry? registry;

        [DebuggerStepThrough]
        private protected ScopeContext(ScopeContext? parentScope) =>
            this.parentScope = parentScope;

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected IVariableInformationRegistry? Registry
        {
            [DebuggerStepThrough]
            get => this.registry;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void CopyInRegistry(
            IVariableInformationRegistry? originateFrom,
            bool force)
        {
            if (force || (this.registry == null))
            {
                this.registry = originateFrom?.Clone();
            }
            else if (
                this.registry is StaticVariableInformationRegistry r &&
                originateFrom is StaticVariableInformationRegistry of)
            {
                // Make faster than reconstruct large registry.
                r.CopyIn(of);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public abstract ITypeCalculator TypeCalculator { get; }

        [DebuggerStepThrough]
        protected internal void MutableBind(
            BoundAttributes attributes,
            IBoundVariableTerm symbol,
            IExpression expression,
            bool ignoreDuplicate)
        {
            this.registry ??= StaticVariableInformationRegistry.Create();
            ((IInternalVariableInformationRegistry)this.registry).Register(
                attributes, symbol, expression, ignoreDuplicate);
        }

        [DebuggerStepThrough]
        public BoundVariables? LookupVariables(string symbol)
        {
            if (((IInternalVariableInformationRegistry?)this.registry)?.Lookup(symbol) is { } results)
            {
                return BoundVariables.Create(results.attributes, results.vis.Memoize());
            }
            else 
            {
                return this.parentScope?.LookupVariables(symbol) ;
            }
        }
    }
}
