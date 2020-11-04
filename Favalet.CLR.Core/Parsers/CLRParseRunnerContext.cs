using System.Diagnostics;
using Favalet.Tokens;

namespace Favalet.Parsers
{
    [DebuggerStepThrough]
    internal sealed class CLRParseRunnerContext : ParseRunnerContext
    {
        public NumericalSignToken? PreSignToken;

        private CLRParseRunnerContext()
        { }

        public new static CLRParseRunnerContext Create() =>
            new CLRParseRunnerContext();
    }
}