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
using Favalet.Ranges;

namespace Favalet.Tokens
{
    partial class Token
    {
        public static Uri UnknownText() =>
            Favalet.Ranges.TextRange.UnknownUri;
        public static TextRange UnknownTextRange() =>
            Favalet.Ranges.TextRange.Unknown;

        public static TextRange TextRange(TextPosition position) =>
            Favalet.Ranges.TextRange.Create(
                Favalet.Ranges.TextRange.UnknownUri,
                position);
        public static TextRange TextRange(TextPosition first, TextPosition last) =>
            Favalet.Ranges.TextRange.Create(
                Favalet.Ranges.TextRange.UnknownUri,
                first,
                last);
        public static TextRange TextRange(Uri text, TextPosition position) =>
            Favalet.Ranges.TextRange.Create(
                text,
                position);
        public static TextRange TextRange(Uri text, TextPosition first, TextPosition last) =>
            Favalet.Ranges.TextRange.Create(
                text,
                first,
                last);
        public static TextRange TextRange(string text, TextPosition position) =>
            Favalet.Ranges.TextRange.Create(
                new Uri(text, UriKind.RelativeOrAbsolute),
                position);
        public static TextRange TextRange(string text, TextPosition first, TextPosition last) =>
            Favalet.Ranges.TextRange.Create(
                new Uri(text, UriKind.RelativeOrAbsolute),
                first,
                last);
        public static TextRange TextRange(int line, int column) =>
            Favalet.Ranges.TextRange.Create(
                Favalet.Ranges.TextRange.UnknownUri,
                TextPosition.Create(line, column));
        public static TextRange TextRange(int firstLine, int firstColumn, int lastLine, int lastColumn) =>
            Favalet.Ranges.TextRange.Create(
                Favalet.Ranges.TextRange.UnknownUri,
                TextPosition.Create(firstLine, firstColumn),
                TextPosition.Create(lastLine, lastColumn));
        public static TextRange TextRange(Uri text, int line, int column) =>
            Favalet.Ranges.TextRange.Create(
                text,
                TextPosition.Create(line, column));
        public static TextRange TextRange(Uri text, int firstLine, int firstColumn, int lastLine, int lastColumn) =>
            Favalet.Ranges.TextRange.Create(
                text,
                TextPosition.Create(firstLine, firstColumn),
                TextPosition.Create(lastLine, lastColumn));
        public static TextRange TextRange(string text, int line, int column) =>
            Favalet.Ranges.TextRange.Create(
                new Uri(text, UriKind.RelativeOrAbsolute),
                TextPosition.Create(line, column));
        public static TextRange TextRange(string text, int firstLine, int firstColumn, int lastLine, int lastColumn) =>
            Favalet.Ranges.TextRange.Create(
                new Uri(text, UriKind.RelativeOrAbsolute),
                TextPosition.Create(firstLine, firstColumn),
                TextPosition.Create(lastLine, lastColumn));
 
        public static IEnumerable<char> OperatorChars =>
            TokenUtilities.operatorChars;

        public static IdentityToken Identity(string identity) =>
            IdentityToken.Create(identity, Favalet.Ranges.TextRange.Unknown);
        public static IdentityToken Identity(string identity, TextRange range) =>
            IdentityToken.Create(identity, range);

        public static NumericalSignToken PlusSign() =>
            NumericalSignToken.Plus(Favalet.Ranges.TextRange.Unknown);
        public static NumericalSignToken PlusSign(TextRange range) =>
            NumericalSignToken.Plus(range);
        
        public static NumericalSignToken MinusSign() =>
            NumericalSignToken.Minus(Favalet.Ranges.TextRange.Unknown);
        public static NumericalSignToken MinusSign(TextRange range) =>
            NumericalSignToken.Minus(range);

        public static OpenParenthesisToken Open(char symbol) =>
            Open(symbol, Favalet.Ranges.TextRange.Unknown);
        public static OpenParenthesisToken Open(char symbol, TextRange range) =>
            TokenUtilities.IsOpenParenthesis(symbol) is ParenthesisPair parenthesis ?
                OpenParenthesisToken.Create(parenthesis, range) :
                throw new InvalidOperationException();

        public static CloseParenthesisToken Close(char symbol) =>
            Close(symbol, Favalet.Ranges.TextRange.Unknown);
        public static CloseParenthesisToken Close(char symbol, TextRange range) =>
            TokenUtilities.IsCloseParenthesis(symbol) is ParenthesisPair parenthesis ?
                CloseParenthesisToken.Create(parenthesis, range) :
                throw new InvalidOperationException();

        public static WhiteSpaceToken WhiteSpace() =>
            WhiteSpaceToken.Create(Favalet.Ranges.TextRange.Unknown);
        public static WhiteSpaceToken WhiteSpace(TextRange range) =>
            WhiteSpaceToken.Create(range);

        public static NumericToken Numeric(string value) =>
            new NumericToken(value, Favalet.Ranges.TextRange.Unknown);
        public static NumericToken Numeric(string value, TextRange range) =>
            new NumericToken(value, range);
    }
}
