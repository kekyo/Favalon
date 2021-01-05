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

namespace Favalet.Contexts
{
    public interface IVariableInformationRegistry
    {
        int Count { get; }
        IVariableInformationRegistry Clone();
    }
    
    internal interface IInternalVariableInformationRegistry :
        IVariableInformationRegistry
    {
        void Register(
            BoundAttributes attributes,
            IBoundVariableTerm variable,
            IExpression expression,
            bool ignoreDuplicate);
        (BoundAttributes attributes, HashSet<VariableInformation> vis)? Lookup(string symbol);
    }
    
    [DebuggerDisplay("{Readable}")]
    [DebuggerStepThrough]
    internal sealed class StaticVariableInformationRegistry :
        IInternalVariableInformationRegistry
    {
        private readonly Dictionary<string, (BoundAttributes attributes, HashSet<VariableInformation> vis)> variables;

        private StaticVariableInformationRegistry(
            Dictionary<string, (BoundAttributes attributes, HashSet<VariableInformation> vis)> variables) =>
            this.variables = variables;

        public int Count =>
            this.variables.Count;

        public IVariableInformationRegistry Clone() =>
            new StaticVariableInformationRegistry(
                new Dictionary<string, (BoundAttributes attributes, HashSet<VariableInformation> vis)>(this.variables));

        public void CopyIn(StaticVariableInformationRegistry originateFrom)
        {
            foreach (var entry in originateFrom.variables)
            {
                this.variables[entry.Key] = entry.Value;
            }
        }
        
        public void Register(
            BoundAttributes attributes,
            IBoundVariableTerm variable,
            IExpression expression,
            bool ignoreDuplicate)
        {
            if (!this.variables.TryGetValue(variable.Symbol, out var entry))
            {
                entry = (attributes, new HashSet<VariableInformation>());
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

            var added = entry.vis.Add(vi);
            if (!ignoreDuplicate && !added)
            {
                throw new InvalidOperationException(
                    $"The symbol already bound: {variable.GetPrettyString(PrettyStringTypes.Minimum)}");
            }
        }

        public (BoundAttributes attributes, HashSet<VariableInformation> vis)? Lookup(string symbol) =>
            this.variables.TryGetValue(symbol, out var entry) ?
                entry :
                default((BoundAttributes attributes, HashSet<VariableInformation> vis)?);

        public string Readable =>
            $"StaticVariableInformationRegistry: Symbols={this.Count}";

        public override string ToString() =>
            this.Readable;

        public static StaticVariableInformationRegistry Create() =>
            new StaticVariableInformationRegistry(
                new Dictionary<string, (BoundAttributes attributes, HashSet<VariableInformation> vis)>());
    }
}
