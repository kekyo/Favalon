using System.Diagnostics;
using Favalet.Expressions;
using System.Runtime.CompilerServices;

namespace Favalet.Parsers
{
    public struct ParseRunnerResult
    {
        public readonly ParseRunner Next;
        public readonly IExpression? Expression;

#if NET45 || NETSTANDARD1_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        private ParseRunnerResult(ParseRunner next, IExpression? expression)
        {
            this.Next = next;
            this.Expression = expression;
        }

#if NET45 || NETSTANDARD1_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        public static ParseRunnerResult Empty(ParseRunner next) =>
            new ParseRunnerResult(next, null);

#if NET45 || NETSTANDARD1_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        public static ParseRunnerResult Create(ParseRunner next, IExpression? expression) =>
            new ParseRunnerResult(next, expression);

#if NET45 || NETSTANDARD1_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        public void Deconstruct(out ParseRunner next, out IExpression? expression)
        {
            next = this.Next;
            expression = this.Expression;
        }
    }
}