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
using System.Linq;
using System.Reflection;

namespace Favalet.Internal
{
    [DebuggerStepThrough]
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
        public static Assembly GetAssembly(this Type type) =>
            type.GetTypeInfo().Assembly;

        public static IEnumerable<Type> GetTypes(this Assembly assembly) =>
            assembly.DefinedTypes.Select(typeInfo => typeInfo.AsType());

        public static IEnumerable<MethodInfo> GetMethods(this Type type) =>
            type.GetTypeInfo().DeclaredMethods;

        public static IEnumerable<PropertyInfo> GetProperties(this Type type) =>
            type.GetTypeInfo().DeclaredProperties;

        public static MethodInfo? GetGetMethod(this PropertyInfo property) =>
            property.GetMethod;

        public static bool IsPublic(this Type type) =>
            type.GetTypeInfo().IsPublic;

        public static bool IsNestedPublic(this Type type) =>
            type.GetTypeInfo().IsNestedPublic;

        public static bool IsGenericType(this Type type) =>
            type.GetTypeInfo().IsGenericType;

        public static bool IsAssignableFrom(this Type lhs, Type rhs) =>
            lhs.GetTypeInfo().IsAssignableFrom(rhs.GetTypeInfo());
#else
        public static Assembly GetAssembly(this Type type) =>
            type.Assembly;

        public static bool IsPublic(this Type type) =>
            type.IsPublic;

        public static bool IsNestedPublic(this Type type) =>
            type.IsNestedPublic;

        public static bool IsGenericType(this Type type) =>
            type.IsGenericType;
#endif

        public static string GetReadableName(this Type type) =>
            readableNames.TryGetValue(type, out var name) ?
                name! :
                type.FullName!;

        public static string GetReadableName(this MemberInfo member) =>
            $"{member.DeclaringType!.GetReadableName()}.{member.Name}";
    }
}
