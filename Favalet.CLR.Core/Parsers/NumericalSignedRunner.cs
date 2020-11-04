using Favalet.Expressions;
using Favalet.Tokens;
using System;
using System.Diagnostics;

namespace Favalet.Parsers
{
    internal sealed class NumericalSignedRunner : ParseRunner
    {
        [DebuggerStepThrough]
        private NumericalSignedRunner()
        { }

        public override ParseRunnerResult Run(
            ParseRunnerContext context,
            ParseRunnerFactory factory,
            Token token)
        {
            Debug.Assert(context is CLRParseRunnerContext);
            Debug.Assert(((CLRParseRunnerContext)context).PreSignToken != null);
            //Debug.Assert(context.ApplyNextAssociative == BoundTermAssociatives.LeftToRight);

            switch (token)
            {
                // "-123"
                case NumericToken numeric:
                    CLRParserUtilities.CombineNumericValue(
                        (CLRParseRunnerContext)context!,
                        numeric);
                    return ParseRunnerResult.Empty(
                        factory.Applying);

                // "-abc"
                case IdentityToken identity:
                    // Will make binary op
                    context!.CombineAfter(VariableTerm.Create(
                        ((CLRParseRunnerContext)context!).PreSignToken!.Symbol.ToString()));
                    context.CombineAfter(
                        VariableTerm.Create(identity.Identity));
                    ((CLRParseRunnerContext)context!).PreSignToken = null;
                    return ParseRunnerResult.Empty(
                        factory.Applying);

                default:
                    throw new InvalidOperationException(token.ToString());
            }
        }

        public static readonly ParseRunner Instance =
            new NumericalSignedRunner();
    }
}
