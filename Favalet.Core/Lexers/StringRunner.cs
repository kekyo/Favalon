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
    internal sealed class StringRunner : LexRunner // TODO: test
    {
        [DebuggerStepThrough]
        private StringRunner()
        { }

        private static StringToken InternalFinish(LexRunnerContext context)
        {
            var (token, range) = context.GetTokenTextAndClear();
            return StringToken.Create(token, range);
        }

        public override LexRunnerResult Run(LexRunnerContext context, Input input)
        {
            if (input.IsNextLine)
            {
                foreach (var ch in Environment.NewLine)
                {
                    context.Append(ch);
                }
                context.ForwardNextLine();
                context.SetStringLastInput(input);
                return LexRunnerResult.Empty(this);
            }
            else if (input.IsDelimiterHint)
            {
                context.SetStringLastInput(input);
                return LexRunnerResult.Empty(this);
            }
            else if (input.IsReset)
            {
                context.ResetAndNextLine();
                return LexRunnerResult.Create(
                    this,
                    ResetToken.Instance);
            }
            else if (context.StringLastInput?.Equals('\\') ?? false)
            {
                switch ((char)input)
                {
                    case 'r':
                        context.Append('\r');
                        break;
                    case 'n':
                        context.Append('\n');
                        break;
                    case 't':
                        context.Append('\t');
                        break;
                    case '\\':
                        context.Append('\\');
                        break;
                    case '\"':
                        context.Append('\"');
                        break;
                    default:
                        context.Append(input);
                        break;
                }
                context.SetStringLastInput(default);
                return LexRunnerResult.Empty(this);
            }
            else if (input == '\\')
            {
                context.ForwardOnly();
                context.SetStringLastInput(input);
                return LexRunnerResult.Empty(this);
            }
            else if (input == '"')
            {
                context.ForwardOnly();
                var token0 = InternalFinish(context);
                context.SetStringLastInput(default);
                return LexRunnerResult.Create(
                    WaitingRunner.Instance,
                    token0);
            }
            else if (!char.IsControl(input))
            {
                context.Append(input);
                context.SetStringLastInput(input);
                return LexRunnerResult.Empty(this);
            }
            else
            {
                context.SetStringLastInput(default);
                throw new InvalidOperationException(input.ToString());
            }
        }

        public override LexRunnerResult Finish(LexRunnerContext context) =>
            LexRunnerResult.Create(WaitingRunner.Instance, InternalFinish(context));

        public static readonly LexRunner Instance =
            new StringRunner();
    }
}
