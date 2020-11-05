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

using Favalet.Internal;
using Favalet.Tokens;
using System;
using System.Diagnostics;

namespace Favalet.Lexers
{
    internal sealed class OperatorRunner : LexRunner
    {
        [DebuggerStepThrough]
        private OperatorRunner()
        { }

        private static Token InternalFinish(LexRunnerContext context, bool forceIdentity)
        {
            var (signText, range0) = context.GetTokenTextAndClear();
            if (!forceIdentity && (signText.Length == 1) &&
                TokenUtilities.IsNumericSign(signText[0]) is NumericalSignes sign)
            {
                return (sign == NumericalSignes.Plus) ?
                    NumericalSignToken.Plus(range0) : NumericalSignToken.Minus(range0);
            }
            else
            {
                return IdentityToken.Create(signText, range0);
            }
        }

        public override LexRunnerResult Run(LexRunnerContext context, Input input)
        {
            if (input.IsNextLine)
            {
                var token0 = InternalFinish(context, true);
                context.ForwardNextLine();
                return LexRunnerResult.Create(
                    WaitingIgnoreSpaceRunner.Instance,
                    token0);
            }
            else if (input.IsDelimiterHint)
            {
                var token0 = InternalFinish(context, true);
                return LexRunnerResult.Create(
                    WaitingRunner.Instance,
                    token0);
            }
            else if (char.IsWhiteSpace(input))
            {
                var token0 = InternalFinish(context, true);
                context.ForwardOnly();
                return LexRunnerResult.Create(
                    WaitingIgnoreSpaceRunner.Instance,
                    token0);
            }
            else if (char.IsDigit(input))
            {
                var token0 = InternalFinish(context, false);
                context.Append(input);
                return LexRunnerResult.Create(NumericRunner.Instance, token0);
            }
            else if (TokenUtilities.IsOpenParenthesis(input) is ParenthesisPair openPair)
            {
                var token0 = InternalFinish(context, true);
                context.ForwardOnly();
                var range1 = context.GetRangeAndClear();
                return LexRunnerResult.Create(
                    WaitingRunner.Instance,
                    token0,
                    OpenParenthesisToken.Create(openPair, range1));
            }
            else if (TokenUtilities.IsCloseParenthesis(input) is ParenthesisPair closePair)
            {
                var token0 = InternalFinish(context, true);
                context.ForwardOnly();
                var range1 = context.GetRangeAndClear();
                return LexRunnerResult.Create(
                    WaitingRunner.Instance,
                    token0,
                    CloseParenthesisToken.Create(closePair, range1));
            }
            else if (TokenUtilities.IsOperator(input))
            {
                context.Append(input);
                return LexRunnerResult.Empty(this);
            }
            else if(!char.IsControl(input))
            {
                var token0 = InternalFinish(context, true);
                context.Append(input);
                return LexRunnerResult.Create(IdentityRunner.Instance, token0);
            }
            else
            {
                throw new InvalidOperationException(input.ToString());
            }
        }

        public override LexRunnerResult Finish(LexRunnerContext context) =>
            LexRunnerResult.Create(WaitingRunner.Instance, InternalFinish(context, true));

        public static readonly LexRunner Instance =
            new OperatorRunner();
    }
}
