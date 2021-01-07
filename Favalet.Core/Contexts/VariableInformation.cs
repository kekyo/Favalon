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
using System.Diagnostics;

namespace Favalet.Contexts
{
    [DebuggerDisplay("{Readable}")]
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
        
        public string Readable =>
#if DEBUG
            $"{this.Symbol}:{this.SymbolHigherOrder.GetPrettyString(PrettyStringTypes.Readable)} --> {this.Expression.GetPrettyString(PrettyStringTypes.Readable)}";
#else
            $"{this.SymbolHigherOrder.GetPrettyString(PrettyStringTypes.Readable)} --> {this.Expression.GetPrettyString(PrettyStringTypes.Readable)}";
#endif

        public override string ToString() =>
            this.Readable;
        
        public static VariableInformation Create(
            string symbol, IExpression symbolHigherOrder, IExpression expression) =>
            new VariableInformation(symbol, symbolHigherOrder, expression);
    }
}
