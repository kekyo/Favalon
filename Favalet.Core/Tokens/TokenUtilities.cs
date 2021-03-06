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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Favalet.Internal;

namespace Favalet.Tokens
{
    [DebuggerStepThrough]
    internal static class TokenUtilities
    {
        private static readonly Dictionary<char, ParenthesisPair> openParenthesis;
        private static readonly Dictionary<char, ParenthesisPair> closeParenthesis;

        internal static readonly HashSet<char> operatorChars = new()
        {
            '!'/* , '"' */, '#', '$', '%', '&' /* , ''' */, /* '(', ')', */
            '*', '+', ',', '-'/* , '.'*/, '/'/*, ':' */, ';', '<', '=', '>', '?',
            '@', /* '[', */ '\\', /* ']', */ '^', '_', '`', /* '{', */ '|', /* '}', */ '~'
        };

        static TokenUtilities()
        {
            // TODO: generate statically
            var parenthesis =
                Enumerable.Range(0x20, ushort.MaxValue - 1).
                    Where(value =>
                        (CharUnicodeInfo.GetUnicodeCategory((char)value) == UnicodeCategory.OpenPunctuation) &&
                        (CharUnicodeInfo.GetUnicodeCategory((char)(value + 1)) == UnicodeCategory.ClosePunctuation)).
                    Select(value => (char)value).
                    Memoize();

            openParenthesis = parenthesis.ToDictionary(
                ch => ch,
                ch => new ParenthesisPair(ch, (char)(ch + 1)));
            openParenthesis.Add('[', new ParenthesisPair('[', ']'));
            openParenthesis.Add('{', new ParenthesisPair('{', '}'));

            closeParenthesis = parenthesis.ToDictionary(
                ch => (char)(ch + 1),
                ch => new ParenthesisPair(ch, (char)(ch + 1)));
            closeParenthesis.Add(']', new ParenthesisPair('[', ']'));
            closeParenthesis.Add('}', new ParenthesisPair('{', '}'));
        }
        
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static NumericalSignes? IsNumericSign(char ch) =>
            ch switch
            {
                '+' => (NumericalSignes?)NumericalSignes.Plus,
                '-' => NumericalSignes.Minus,
                _ => null
            };

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ParenthesisPair? IsOpenParenthesis(char ch) =>
            openParenthesis.TryGetValue(ch, out var p) ? (ParenthesisPair?)p : null;

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ParenthesisPair? IsCloseParenthesis(char ch) =>
            closeParenthesis.TryGetValue(ch, out var p) ? (ParenthesisPair?)p : null;

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsOperator(char ch) =>
            operatorChars.Contains(ch);
    }
}
