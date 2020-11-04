using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Favalet.Internal
{
    internal static class StringUtilities
    {
#if NET35
        [DebuggerStepThrough]
        public static string Join(string separator, IEnumerable<string> values) =>
            string.Join(separator, values.Memoize());
#else
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        public static string Join(string separator, IEnumerable<string> values) =>
            string.Join(separator, values);
#endif
    }
}
