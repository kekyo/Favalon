using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Favalet.Internal
{
    internal static class ArrayEx
    {
#if NETSTANDARD2_0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        public static T[] Empty<T>() =>
            Array.Empty<T>();
#else
        private static class EmptyHolder<T>
        {
            public static readonly T[] Empty = new T[0];
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [DebuggerStepThrough]
        public static T[] Empty<T>() =>
            EmptyHolder<T>.Empty;
#endif
    }
}
