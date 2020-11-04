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

namespace Favalet.Tokens
{
    public enum NumericalSignes
    {
        Unknown = 0,
        Plus = 1,
        Minus = -1
    }

    public sealed class NumericalSignToken :
        SymbolToken
    {
        public readonly NumericalSignes Sign;

        private NumericalSignToken(NumericalSignes sign) =>
            this.Sign = sign;

        public override char Symbol =>
            this.Sign switch
            {
                NumericalSignes.Plus => '+',
                NumericalSignes.Minus => '-',
                _ => '?'
            };

        public override int GetHashCode() =>
            this.Sign.GetHashCode();

        public bool Equals(NumericalSignToken? other) =>
            other?.Sign.Equals(this.Sign) ?? false;

        public override bool Equals(object obj) =>
            this.Equals(obj as NumericalSignToken);

        public void Deconstruct(out NumericalSignes sign) =>
            sign = this.Sign;

        internal static readonly NumericalSignToken Plus =
            new NumericalSignToken(NumericalSignes.Plus);
        internal static readonly NumericalSignToken Minus =
            new NumericalSignToken(NumericalSignes.Minus);
    }
}
