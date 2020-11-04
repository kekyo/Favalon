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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Favalet.Internal
{
    internal static class ReflectionExtension
    {
        private static readonly Dictionary<Type, string> readableNames =
            new Dictionary<Type, string>
            {
                { typeof(void), "void" },
                { typeof(bool), "bool" },
                { typeof(byte), "byte" },
                { typeof(sbyte), "sbyte" },
                { typeof(short), "short" },
                { typeof(ushort), "ushort" },
                { typeof(int), "int" },
                { typeof(uint), "uint" },
                { typeof(long), "long" },
                { typeof(ulong), "ulong" },
                { typeof(float), "float" },
                { typeof(double), "double" },
                { typeof(decimal), "decimal" },
                { typeof(char), "char" },
                { typeof(string), "string" },
                { typeof(object), "object" },
            };

#if NETSTANDARD1_1
        [DebuggerStepThrough]
        public static bool IsAssignableFrom(this Type lhs, Type rhs) =>
            lhs.GetTypeInfo().IsAssignableFrom(rhs.GetTypeInfo());
#endif

        [DebuggerStepThrough]
        public static string GetReadableName(this Type type) =>
            readableNames.TryGetValue(type, out var name) ?
                name :
                type.FullName;

        [DebuggerStepThrough]
        public static string GetReadableName(this MethodBase method) =>
            $"{method.DeclaringType.GetReadableName()}.{method.Name}";
    }
}
