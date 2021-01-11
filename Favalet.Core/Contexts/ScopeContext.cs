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

using Favalet.Expressions;
using Favalet.Expressions.Specialized;
using Favalet.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

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
        private StaticVariableInformationRegistry? defaultRegistry;
        private List<IVariableInformationRegistry>? registries;

        [DebuggerStepThrough]
        private protected ScopeContext(ScopeContext? parentScope) =>
            this.parentScope = parentScope;

        public abstract ITypeCalculator TypeCalculator { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected StaticVariableInformationRegistry? DefaultRegistry
        {
            [DebuggerStepThrough]
            get => this.defaultRegistry;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void CopyInDefaultRegistry(
            StaticVariableInformationRegistry? originateFrom,
            bool force)
        {
            if (force || (this.defaultRegistry == null))
            {
                this.defaultRegistry = originateFrom?.Clone();
                if (this.defaultRegistry != null)
                {
                    Debug.Assert(this.registries == null);
                    this.registries = new List<IVariableInformationRegistry> {this.defaultRegistry};
                }
            }
            else if (originateFrom != null)
            {
                // Make faster than reconstruct large registry.
                this.defaultRegistry.CopyIn(originateFrom);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        [DebuggerStepThrough]
        private StaticVariableInformationRegistry PrepareRegistry()
        {
            if (this.defaultRegistry == null)
            {
                Debug.Assert(this.registries == null);
                this.defaultRegistry = StaticVariableInformationRegistry.Create();
                this.registries = new List<IVariableInformationRegistry> {this.defaultRegistry};
            }
            return this.defaultRegistry;
        }

        [DebuggerStepThrough]
        public void MutableBind(IVariableInformationRegistry registry)
        {
            this.PrepareRegistry();
            Debug.Assert(this.registries != null);
            
            this.registries!.Add(registry);
        }

        [DebuggerStepThrough]
        protected internal void MutableBind(
            IBoundVariableTerm symbol,
            IExpression expression,
            bool ignoreDuplicate) =>
            this.PrepareRegistry().Register(symbol, expression, ignoreDuplicate);

        [DebuggerStepThrough]
        public BoundVariables? LookupVariables(string symbol)
        {
            if (this.registries != null)
            {
                Debug.Assert(this.defaultRegistry != null);
                Debug.Assert(this.registries.Count >= 1);
                
                // Also will make faster lookup.
                if (this.registries.Count == 1)
                {
                    Debug.Assert(object.ReferenceEquals(this.defaultRegistry, this.registries[0]));
                
                    // Matched from default registry.
                    if (this.defaultRegistry!.Lookup(symbol) is { } results)
                    {
                        return BoundVariables.Create(results.attributes, results.vis.Memoize());
                    }
                }
                else
                {
                    // Combined all registries.
                    var combined = this.registries.
                        Collect(registry => registry.Lookup(symbol)).
                        GroupBy(result => result.attributes).
#if DEBUG
                        Select(g => (attr: g.Key, vis: g.SelectMany(result => result.vis.Memoize()))).
#else
                        Select(g => (attr: g.Key, vis: g.SelectMany(result => result.vis))).
#endif
                        Memoize();
                    if (combined.Length >= 1)
                    {
                        // TODO: The bound attributes can be applicable only one.
                        Debug.Assert(combined.Length == 1);
                        
                        return BoundVariables.Create(combined[0].attr, combined[0].vis.Distinct().Memoize());
                    }
                }
            }

            // Fallback into parent.
            return this.parentScope?.LookupVariables(symbol) ;
        }
    }
}
