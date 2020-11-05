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
using Favalet.Expressions;
using Favalet.Tokens;

namespace Favalet.Parsers
{
    internal static class CLRParserUtilities
    {
        public static void SetNumericalSign(
            CLRParseRunnerContext context,
            NumericalSignToken numericSign) =>
            context.PreSignToken = numericSign;
        
        public static void CombineNumericValue(
            CLRParseRunnerContext context,
            NumericToken numeric)
        {
            var sign = context.PreSignToken?.Sign == NumericalSignes.Minus ? -1 : 1;
            if (int.TryParse(numeric.Value, out var intValue))
            {
                context.CombineAfter(ConstantTerm.From(intValue * sign, numeric.Range));  // TODO: range
                context.PreSignToken = null;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Couldn't parse numeric: {numeric.Value}");
            }
        }
    }
}
