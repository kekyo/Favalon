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
    internal sealed class WaitingRunner : LexRunner
    {
        [DebuggerStepThrough]
        private WaitingRunner()
        { }

        public override LexRunnerResult Run(LexRunnerContext context, Input input)
        {
            if (char.IsWhiteSpace(input))
            {
                return LexRunnerResult.Create(
                    WaitingIgnoreSpaceRunner.Instance,
                    Token.WhiteSpace());
            }
            else if (char.IsDigit(input))
            {
                context.TokenBuffer.Append(input);
                return LexRunnerResult.Empty(NumericRunner.Instance);
            }
            else if (TokenUtilities.IsOpenParenthesis(input) is ParenthesisPair)
            {
                return LexRunnerResult.Create(
                    this,
                    Token.Open(input));
            }
            else if (TokenUtilities.IsCloseParenthesis(input) is ParenthesisPair)
            {
                return LexRunnerResult.Create(
                    this,
                    Token.Close(input));
            }
            else if (TokenUtilities.IsOperator(input))
            {
                context.TokenBuffer.Append(input);
                return LexRunnerResult.Empty(OperatorRunner.Instance);
            }
            else if (!char.IsControl(input))
            {
                context.TokenBuffer.Append(input);
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
