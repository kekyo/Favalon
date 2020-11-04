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
            var token = context.TokenBuffer.ToString();
            context.TokenBuffer.Clear();
            if (!forceIdentity && (token.Length == 1) &&
                TokenUtilities.IsNumericSign(token[0]) is NumericalSignes sign)
            {
                return (sign == NumericalSignes.Plus) ?
                    NumericalSignToken.Plus : NumericalSignToken.Minus;
            }
            else
            {
                return new IdentityToken(token);
            }
        }

        public override LexRunnerResult Run(LexRunnerContext context, char ch)
        {
            if (char.IsWhiteSpace(ch))
            {
                var token0 = InternalFinish(context, true);
                context.TokenBuffer.Clear();
                return LexRunnerResult.Create(WaitingIgnoreSpaceRunner.Instance, token0, WhiteSpaceToken.Instance);
            }
            else if (char.IsDigit(ch))
            {
                var token0 = InternalFinish(context, false);
                context.TokenBuffer.Append(ch);
                return LexRunnerResult.Create(NumericRunner.Instance, token0);
            }
            else if (TokenUtilities.IsOpenParenthesis(ch) is ParenthesisPair)
            {
                return LexRunnerResult.Create(
                    WaitingRunner.Instance,
                    InternalFinish(context, true),
                    Token.Open(ch));
            }
            else if (TokenUtilities.IsCloseParenthesis(ch) is ParenthesisPair)
            {
                return LexRunnerResult.Create(
                    WaitingRunner.Instance,
                    InternalFinish(context, true),
                    Token.Close(ch));
            }
            else if (TokenUtilities.IsOperator(ch))
            {
                context.TokenBuffer.Append(ch);
                return LexRunnerResult.Empty(this);
            }
            else if(!char.IsControl(ch))
            {
                var token0 = InternalFinish(context, true);
                context.TokenBuffer.Append(ch);
                return LexRunnerResult.Create(IdentityRunner.Instance, token0);
            }
            else
            {
                throw new InvalidOperationException(ch.ToString());
            }
        }

        public override LexRunnerResult Finish(LexRunnerContext context) =>
            LexRunnerResult.Create(WaitingRunner.Instance, InternalFinish(context, true));

        public static readonly LexRunner Instance =
            new OperatorRunner();
    }
}
