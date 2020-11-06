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
    internal sealed class WaitingIgnoreSpaceRunner : LexRunner
    {
        [DebuggerStepThrough]
        private WaitingIgnoreSpaceRunner()
        { }

        public override LexRunnerResult Run(LexRunnerContext context, Input input)
        {
            if (input.IsNextLine)
            {
                context.ForwardNextLine();
                return LexRunnerResult.Empty(this);
            }
            else if (input.IsDelimiterHint)
            {
                var range0 = context.GetRangeAndClear();
                return LexRunnerResult.Create(
                    this,
                    WhiteSpaceToken.Create(range0),
                    DelimiterHintToken.Instance);
            }
            else if (char.IsWhiteSpace(input))
            {
                context.ForwardOnly();
                return LexRunnerResult.Empty(this);
            }
            else if (char.IsDigit(input))
            {
                var range0 = context.GetRangeAndClear();
                context.Append(input);
                return LexRunnerResult.Create(
                    NumericRunner.Instance,
                    WhiteSpaceToken.Create(range0));
            }
            else if (TokenUtilities.IsOpenParenthesis(input) is ParenthesisPair openPair)
            {
                var range0 = context.GetRangeAndClear();
                context.ForwardOnly();
                var range1 = context.GetRangeAndClear();
                return LexRunnerResult.Create(
                    WaitingRunner.Instance,
                    WhiteSpaceToken.Create(range0),
                    OpenParenthesisToken.Create(openPair, range0));
            }
            else if (TokenUtilities.IsCloseParenthesis(input) is ParenthesisPair closePair)
            {
                var range0 = context.GetRangeAndClear();
                context.ForwardOnly();
                var range1 = context.GetRangeAndClear();
                return LexRunnerResult.Create(
                    WaitingRunner.Instance,
                    WhiteSpaceToken.Create(range0),
                    CloseParenthesisToken.Create(closePair, range0));
            }
            else if (TokenUtilities.IsOperator(input))
            {
                var range0 = context.GetRangeAndClear();
                context.Append(input);
                return LexRunnerResult.Create(
                    OperatorRunner.Instance,
                    WhiteSpaceToken.Create(range0));
            }
            else if (!char.IsControl(input))
            {
                var range0 = context.GetRangeAndClear();
                context.Append(input);
                return LexRunnerResult.Create(
                    IdentityRunner.Instance,
                    WhiteSpaceToken.Create(range0));
            }
            else
            {
                throw new InvalidOperationException(input.ToString());
            }
        }

        public override LexRunnerResult Finish(LexRunnerContext context)
        {
            var range0 = context.GetRangeAndClear();
            return LexRunnerResult.Create(
                WaitingRunner.Instance,
                WhiteSpaceToken.Create(range0));
        }

        public static readonly LexRunner Instance =
            new WaitingIgnoreSpaceRunner();
    }
}
