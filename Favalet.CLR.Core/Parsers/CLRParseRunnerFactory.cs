using System.Diagnostics;

namespace Favalet.Parsers
{
    [DebuggerStepThrough]
    internal sealed class CLRParseRunnerFactory :
        ParseRunnerFactory
    {
        private CLRParseRunnerFactory()
        { }

        public override ParseRunnerContext CreateContext() =>
            CLRParseRunnerContext.Create();

        public override ParseRunner Waiting =>
            CLRWaitingRunner.Instance;
        public override ParseRunner Applying =>
            CLRApplyingRunner.Instance;

        public new static readonly CLRParseRunnerFactory Instance =
            new CLRParseRunnerFactory();
    }
}