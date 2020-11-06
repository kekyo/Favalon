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

using Favalet.Expressions;
using Favalet.Expressions.Specialized;
using Favalet.Internal;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Favalet.Contexts
{
    public readonly struct VariableInformation
    {
#if DEBUG
        public readonly string Symbol;
#endif
        public readonly IExpression SymbolHigherOrder;
        public readonly IExpression Expression;

        private VariableInformation(
            string symbol, IExpression symbolHigherOrder, IExpression expression)
        {
#if DEBUG
            this.Symbol = symbol;
#endif
            this.SymbolHigherOrder = symbolHigherOrder;
            this.Expression = expression;
        }

        public override string ToString() =>
#if DEBUG
            $"{this.Symbol}:{this.SymbolHigherOrder.GetPrettyString(PrettyStringTypes.Readable)} --> {this.Expression.GetPrettyString(PrettyStringTypes.Readable)}";
#else
            $"{this.SymbolHigherOrder.GetPrettyString(PrettyStringTypes.Readable)} --> {this.Expression.GetPrettyString(PrettyStringTypes.Readable)}";
#endif
        public static VariableInformation Create(
            string symbol, IExpression symbolHigherOrder, IExpression expression) =>
            new VariableInformation(symbol, symbolHigherOrder, expression);
    }

    public interface IScopeContext
    {
        ITypeCalculator TypeCalculator { get; }

        VariableInformation[] LookupVariables(string symbol);
    }

    public abstract class ScopeContext :
        IScopeContext
    {
        private readonly ScopeContext? parent;
        private Dictionary<string, List<VariableInformation>>? variables;

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
            this.variables ??= new Dictionary<string, List<VariableInformation>>();

            if (!this.variables.TryGetValue(symbol.Symbol, out var list))
            {
                list = new List<VariableInformation>();
                this.variables.Add(symbol.Symbol, list);
            }

            var vi = VariableInformation.Create(
                symbol.Symbol,
                symbol.HigherOrder,
                expression);

            if (checkDuplicate)
            {
                if (!list.Any(entry => entry.Equals(vi)))
                {
                    list.Add(vi);
                }
            }
            else
            {
                Debug.Assert(!list.Any(entry => entry.Equals(vi)));
                list.Add(vi);
            }
        }

        public VariableInformation[] LookupVariables(string symbol)
        {
            if (this.variables != null &&
                this.variables.TryGetValue(symbol, out var list))
            {
                return list.Memoize();
            }
            else
            {
                return
                    this.parent?.LookupVariables(symbol) ??
                    ArrayEx.Empty<VariableInformation>();
            }
        }
    }
}
