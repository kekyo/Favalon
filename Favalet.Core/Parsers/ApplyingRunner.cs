using Favalet.Expressions;
using Favalet.Tokens;
using System;
using System.Diagnostics;

namespace Favalet.Parsers
{
    public class ApplyingRunner : ParseRunner
    {
        [DebuggerStepThrough]
        protected ApplyingRunner()
        { }

        public override ParseRunnerResult Run(
            ParseRunnerContext context,
            ParseRunnerFactory factory,
            Token token)
        {
            Debug.Assert(context.Current != null);
            //Debug.Assert(context.PreSignToken == null);

            switch (token)
            {
                case WhiteSpaceToken _:
                    return ParseRunnerResult.Empty(this);
                
                case IdentityToken identity:
                    context.CombineAfter(VariableTerm.Create(identity.Identity));
                    return ParseRunnerResult.Empty(factory.Applying);

                case OpenParenthesisToken parenthesis:
                    context.PushScope(parenthesis.Pair);
                    return ParseRunnerResult.Empty(factory.Waiting);
                
                case CloseParenthesisToken parenthesis:
                    context.PopScope(parenthesis.Pair);
                    return ParseRunnerResult.Empty(factory.Applying);

                default:
                    throw new InvalidOperationException(token.ToString());
            }
        }

        internal static readonly ParseRunner Instance =
            new ApplyingRunner();
    }
}
