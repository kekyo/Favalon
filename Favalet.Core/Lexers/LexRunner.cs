using System.Diagnostics;

namespace Favalet.Lexers
{
    internal abstract class LexRunner
    {
        [DebuggerStepThrough]
        protected LexRunner()
        { }

        public abstract LexRunnerResult Run(LexRunnerContext context, char ch);

        public virtual LexRunnerResult Finish(LexRunnerContext context) =>
            LexRunnerResult.Empty(this);
    }
}
