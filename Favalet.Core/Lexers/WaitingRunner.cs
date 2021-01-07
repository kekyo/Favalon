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

using Favalet.Tokens;
using System;
using System.Diagnostics;

namespace Favalet.Lexers
{
    internal sealed class WaitingRunner : LexRunner
    {
        [DebuggerStepThrough]
        private WaitingRunner()
        { }

        public override LexRunnerResult Run(LexRunnerContext context, Input input)
        {
            if (input.IsNextLine)
            {
                context.ForwardNextLine();
                return LexRunnerResult.Empty(
                    WaitingIgnoreSpaceRunner.Instance);
            }
            else if (input.IsDelimiterHint)
            {
                return LexRunnerResult.Create(
                    this,
                    DelimiterHintToken.Instance);
            }
            else if (input.IsReset)
            {
                context.ResetAndNextLine();
                return LexRunnerResult.Create(
                    this,
                    ResetToken.Instance);
            }
            else if (char.IsWhiteSpace(input))
            {
                context.ForwardOnly();
                return LexRunnerResult.Empty(
                    WaitingIgnoreSpaceRunner.Instance);
            }
            else if (input == '"')
            {
                context.ForwardOnly();
                return LexRunnerResult.Empty(
                    StringRunner.Instance);
            }
            else if (char.IsDigit(input))
            {
                context.Append(input);
                return LexRunnerResult.Empty(NumericRunner.Instance);
            }
            else if (TokenUtilities.IsOpenParenthesis(input) is { } openPair)
            {
                context.ForwardOnly();
                var range0 = context.GetRangeAndClear();
                return LexRunnerResult.Create(
                    this,
                    OpenParenthesisToken.Create(openPair, range0));
            }
            else if (TokenUtilities.IsCloseParenthesis(input) is { } closePair)
            {
                context.ForwardOnly();
                var range0 = context.GetRangeAndClear();
                return LexRunnerResult.Create(
                    this,
                    CloseParenthesisToken.Create(closePair, range0));
            }
            else if (TokenUtilities.IsOperator(input))
            {
                context.Append(input);
                return LexRunnerResult.Empty(OperatorRunner.Instance);
            }
            else if (!char.IsControl(input))
            {
                context.Append(input);
                return LexRunnerResult.Empty(IdentityRunner.Instance);
            }
            else
            {
                throw new InvalidOperationException(input.ToString());
            }
        }

        public static readonly LexRunner Instance =
            new WaitingRunner();
    }
}
