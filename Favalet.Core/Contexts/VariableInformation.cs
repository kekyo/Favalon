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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Favalet.Contexts
{
    [DebuggerStepThrough]
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
    
    [DebuggerStepThrough]
    public sealed class VariableInformationRegistry
    {
        private readonly Dictionary<string, (BoundAttributes attributes, List<VariableInformation> list)> variables;

        private VariableInformationRegistry(
            Dictionary<string, (BoundAttributes attributes, List<VariableInformation> list)> variables) =>
            this.variables = variables;

        public VariableInformationRegistry Clone() =>
            new VariableInformationRegistry(
                new Dictionary<string, (BoundAttributes attributes, List<VariableInformation> list)>(this.variables));

        internal void CopyIn(VariableInformationRegistry originateFrom)
        {
            foreach (var entry in originateFrom.variables)
            {
                this.variables[entry.Key] = entry.Value;
            }
        }
        
        internal void Register(
            BoundAttributes attributes,
            IBoundVariableTerm variable,
            IExpression expression,
            bool checkDuplicate)
        {
            if (!this.variables.TryGetValue(variable.Symbol, out var entry))
            {
                entry = (attributes, new List<VariableInformation>());
                this.variables.Add(variable.Symbol, entry);
            }

            if (entry.attributes != attributes)
            {
                throw new ArgumentException(
                    $"Couldn't change bound attributes: {entry.attributes} --> {attributes}");
            }

            var vi = VariableInformation.Create(
                variable.Symbol,
                variable.HigherOrder,
                expression);

            if (checkDuplicate)
            {
                if (!entry.list.Any(vi_ => vi_.Equals(vi)))
                {
                    entry.list.Add(vi);
                }
            }
            else
            {
                Debug.Assert(!entry.list.Any(vi_ => vi_.Equals(vi)));
                entry.list.Add(vi);
            }
        }

        internal (BoundAttributes attributes, List<VariableInformation> vis)? Lookup(string symbol) =>
            this.variables.TryGetValue(symbol, out var entry) ?
                entry :
                default((BoundAttributes attributes, List<VariableInformation> vis)?);
        
        internal static VariableInformationRegistry Create() =>
            new VariableInformationRegistry(
                new Dictionary<string, (BoundAttributes attributes, List<VariableInformation> list)>());
    }
}