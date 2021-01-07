/////////////////////////////////////////////////////////////////////////////////////////////////
//
// Favalon - An Interactive Shell Based on a Typed Lambda Calculus.
// Copyright (c) 2018-2020 Kouji Matsui (@kozy_kekyo, @kekyo2)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
/////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Favalet.Internal
{
    [DebuggerStepThrough]
    internal static class StringUtilities
    {
#if NET35
        public static void Clear(this StringBuilder sb) =>
            sb.Remove(0, sb.Length);
        
        public static string Join(string separator, IEnumerable<string> values) =>
            string.Join(separator, values.Memoize());

        public static bool IsNullOrWhiteSpace(string? str) =>
            string.IsNullOrEmpty(str) || str!.All(char.IsWhiteSpace);
#else
#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string Join(string separator, IEnumerable<string> values) =>
            string.Join(separator, values);

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsNullOrWhiteSpace(string? str) =>
            string.IsNullOrEmpty(str);
#endif
    }
}
