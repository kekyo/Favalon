using Favalet.Expressions;
using Favalet.Tokens;
using System;
using System.Diagnostics;

namespace Favalet.Parsers
{
    internal sealed class CLRWaitingRunner : WaitingRunner
    {
        [DebuggerStepThrough]
        private CLRWaitingRunner()
        { }

        public override ParseRunnerResult Run(
            ParseRunnerContext context,
            ParseRunnerFactory factory,
            Token token)
        {
            Debug.Assert(context is CLRParseRunnerContext);
            Debug.Assert(context.Current == null);
            Debug.Assert(((CLRParseRunnerContext)context).PreSignToken == null);

            switch (token)
            {
                // "123"
                case NumericToken numeric:
                    CLRParserUtilities.CombineNumericValue(
                        (CLRParseRunnerContext)context!,
                        numeric);
                    return ParseRunnerResult.Empty(factory.Applying);
                
                // "-"
                case NumericalSignToken numericSign:
                    ((CLRParseRunnerContext)context!).PreSignToken = numericSign;
                    return ParseRunnerResult.Empty(NumericalSignedRunner.Instance);

                default:
                    return base.Run(context!, factory, token);
            }
        }

        internal new static readonly ParseRunner Instance =
            new CLRWaitingRunner();
    }
}
