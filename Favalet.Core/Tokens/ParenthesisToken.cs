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
using System.Diagnostics;
using Favalet.Ranges;

namespace Favalet.Tokens
{
    [DebuggerStepThrough]
    public struct ParenthesisPair
    {
        public readonly char Open;
        public readonly char Close;

        internal ParenthesisPair(char open, char close)
        {
            this.Open = open;
            this.Close = close;
        }

        public override string ToString() =>
            $"'{this.Open}','{this.Close}'";
    }

    [DebuggerStepThrough]
    public abstract class ParenthesisToken :
        SymbolToken, IEquatable<ParenthesisToken?>
    {
        public readonly ParenthesisPair Pair;

        private protected ParenthesisToken(ParenthesisPair parenthesis, TextRange range) :
            base(range) =>
            this.Pair = parenthesis;

        public override int GetHashCode() =>
            this.Pair.GetHashCode();

        public bool Equals(ParenthesisToken? other) =>
            other?.Pair.Equals(this.Pair) ?? false;

        public override bool Equals(object obj) =>
            this.Equals(obj as ParenthesisToken);

        public void Deconstruct(out ParenthesisPair parenthesis) =>
            parenthesis = this.Pair;
        public void Deconstruct(out ParenthesisPair parenthesis, out TextRange range)
        {
            parenthesis = this.Pair;
            range = this.Range;
        }
    }
}
