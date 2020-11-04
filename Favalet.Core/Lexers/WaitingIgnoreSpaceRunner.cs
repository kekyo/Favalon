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

        public override LexRunnerResult Run(LexRunnerContext context, char ch)
        {
            if (char.IsWhiteSpace(ch))
            {
                return LexRunnerResult.Empty(this);
            }
            else if (char.IsDigit(ch))
            {
                context.TokenBuffer.Append(ch);
                return LexRunnerResult.Empty(NumericRunner.Instance);
            }
            else if (TokenUtilities.IsOpenParenthesis(ch) is ParenthesisPair)
            {
                return LexRunnerResult.Create(
                    WaitingRunner.Instance,
                    Token.Open(ch));
            }
            else if (TokenUtilities.IsCloseParenthesis(ch) is ParenthesisPair)
            {
                return LexRunnerResult.Create(
                    WaitingRunner.Instance,
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
            new WaitingIgnoreSpaceRunner();
    }
}
