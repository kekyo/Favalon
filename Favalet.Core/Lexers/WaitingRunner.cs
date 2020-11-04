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

        public override LexRunnerResult Run(LexRunnerContext context, char ch)
        {
            if (char.IsWhiteSpace(ch))
            {
                return LexRunnerResult.Create(
                    WaitingIgnoreSpaceRunner.Instance,
                    WhiteSpaceToken.Instance);
            }
            else if (char.IsDigit(ch))
            {
                context.TokenBuffer.Append(ch);
                return LexRunnerResult.Empty(NumericRunner.Instance);
            }
            else if (TokenUtilities.IsOpenParenthesis(ch) is ParenthesisPair)
            {
                return LexRunnerResult.Create(
                    this,
                    Token.Open(ch));
            }
            else if (TokenUtilities.IsCloseParenthesis(ch) is ParenthesisPair)
            {
                return LexRunnerResult.Create(
                    this,
                    Token.Close(ch));
            }
            else if (TokenUtilities.IsOperator(ch))
            {
                context.TokenBuffer.Append(ch);
                return LexRunnerResult.Empty(OperatorRunner.Instance);
            }
            else if (!char.IsControl(ch))
            {
                context.TokenBuffer.Append(ch);
                return LexRunnerResult.Empty(IdentityRunner.Instance);
            }
            else
            {
                throw new InvalidOperationException(ch.ToString());
            }
        }

        public static readonly LexRunner Instance =
            new WaitingRunner();
    }
}
