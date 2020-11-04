using System.Diagnostics;
using Favalet.Tokens;

namespace Favalet.Parsers
{
    [DebuggerStepThrough]
    public abstract class ParseRunner
    {
        protected ParseRunner()
        { }

        public abstract ParseRunnerResult Run(
            ParseRunnerContext context,
            ParseRunnerFactory factory,
            Token token);
    }
}