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
using Favalet.Expressions.Algebraic;
using Favalet.Expressions.Specialized;
using Favalet.Ranges;
using Favalet.Internal;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Favalet.Contexts
{
    public interface IScopeContext
    {
        ITypeCalculator TypeCalculator { get; }

        IEnumerable<VariableInformation> LookupVariables(string symbol);
    }

    public abstract class ScopeContext :
        IScopeContext
    {
        private readonly ScopeContext? parent;
        private VariableInformationRegistry? registry;

        [DebuggerStepThrough]
        internal ScopeContext(ScopeContext? parent, ITypeCalculator typeCalculator)
        {
            this.parent = parent;
            this.TypeCalculator = typeCalculator;
        }

        public ITypeCalculator TypeCalculator { get; }

        private protected void MutableBind(
            IBoundVariableTerm symbol, IExpression expression, bool checkDuplicate)
        {
            this.registry ??= new VariableInformationRegistry();
            this.registry.Register(symbol, expression, checkDuplicate);
        }

        public IEnumerable<VariableInformation> LookupVariables(string symbol)
        {
            var overrideVariables =
                this.registry?.Lookup(symbol) ??
                ArrayEx.Empty<VariableInformation>();
            return (overrideVariables.Length >= 1) ?
                overrideVariables :
                this.parent?.LookupVariables(symbol) ??
                    Enumerable.Empty<VariableInformation>();
        }
    }
}
