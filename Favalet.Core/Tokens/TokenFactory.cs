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
using System.Collections.Generic;

namespace Favalet.Tokens
{
    partial class Token
    {
        public static IEnumerable<char> OperatorChars =>
            TokenUtilities.operatorChars;

        public static IdentityToken Identity(string identity) =>
            new IdentityToken(identity);

        public static NumericalSignToken PlusSign() =>
            NumericalSignToken.Plus;
        public static NumericalSignToken MinusSign() =>
            NumericalSignToken.Minus;

        public static OpenParenthesisToken Open(char symbol) =>
            TokenUtilities.IsOpenParenthesis(symbol) is ParenthesisPair parenthesis ?
                new OpenParenthesisToken(parenthesis) :
                throw new InvalidOperationException();

        public static CloseParenthesisToken Close(char symbol) =>
            TokenUtilities.IsCloseParenthesis(symbol) is ParenthesisPair parenthesis ?
                new CloseParenthesisToken(parenthesis) :
                throw new InvalidOperationException();

        public static WhiteSpaceToken WhiteSpace() =>
            WhiteSpaceToken.Instance;

        public static NumericToken Numeric(string value) =>
            new NumericToken(value);
    }
}
