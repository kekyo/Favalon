using Favalet.Internal;
using Favalet.Tokens;
using System;
using System.Diagnostics;

namespace Favalet.Lexers
{
    internal sealed class IdentityRunner : LexRunner
    {
        [DebuggerStepThrough]
        private IdentityRunner()
        { }

        private static IdentityToken InternalFinish(LexRunnerContext context)
        {
            var token = context.TokenBuffer.ToString();
            context.TokenBuffer.Clear();
            return new IdentityToken(token);
        }

        public override LexRunnerResult Run(LexRunnerContext context, char ch)
        {
            if (char.IsWhiteSpace(ch))
            {
                var token = context.TokenBuffer.ToString();
                context.TokenBuffer.Clear();
                return LexRunnerResult.Create(WaitingIgnoreSpaceRunner.Instance, new IdentityToken(token), WhiteSpaceToken.Instance);
            }
            else if (TokenUtilities.IsOpenParenthesis(ch) is ParenthesisPair)
            {
                return LexRunnerResult.Create(
                    WaitingRunner.Instance,
                    InternalFinish(context),
                    Token.Open(ch));
            }
            else if (TokenUtilities.IsCloseParenthesis(ch) is ParenthesisPair)
            {
                return LexRunnerResult.Create(
                    WaitingRunner.Instance,
                    InternalFinish(context),
                    Token.Close(ch));
            }
            else if (TokenUtilities.IsOperator(ch))
            {
                var token0 = InternalFinish(context);
                context.TokenBuffer.Append(ch);
                return LexRunnerResult.Create(OperatorRunner.Instance, token0);
            }
            else if (!char.IsControl(ch))
            {
                context.TokenBuffer.Append(ch);
                return LexRunnerResult.Empty(this);
            }
            else
            {
                throw new InvalidOperationException(ch.ToString());
            }
        }

        public override LexRunnerResult Finish(LexRunnerContext context) =>
            LexRunnerResult.Create(WaitingRunner.Instance, InternalFinish(context));

        public static readonly LexRunner Instance =
            new IdentityRunner();
    }
}
