using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Favalet.Lexers
{
    internal sealed class LexRunnerContext
    {
        public readonly StringBuilder TokenBuffer;

#if NET45 || NETSTANDARD1_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        private LexRunnerContext(StringBuilder tokenBuffer) =>
            this.TokenBuffer = tokenBuffer;

#if NET45 || NETSTANDARD1_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        public static LexRunnerContext Create() =>
            new LexRunnerContext(new StringBuilder());
    }
}
