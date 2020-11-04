using System.Diagnostics;

namespace Favalet.Parsers
{
    [DebuggerStepThrough]
    public class ParseRunnerFactory
    {
        protected ParseRunnerFactory()
        { }

        public virtual ParseRunnerContext CreateContext() =>
            ParseRunnerContext.Create();
        
        public virtual ParseRunner Waiting =>
            WaitingRunner.Instance;
        public virtual ParseRunner Applying =>
            ApplyingRunner.Instance;

        public static readonly ParseRunnerFactory Instance =
            new ParseRunnerFactory();
    }
}