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
#if NETSTANDARD1_1
        public static Assembly GetAssembly(this Type type) =>
            type.GetTypeInfo().Assembly;

        public static IEnumerable<Type> GetTypes(this Assembly assembly) =>
            assembly.DefinedTypes.Select(typeInfo => typeInfo.AsType());

        public static IEnumerable<ConstructorInfo> GetDeclaredConstructors(this Type type) =>
            type.GetTypeInfo().DeclaredConstructors;

        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type) =>
            type.GetTypeInfo().DeclaredMethods;

        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type type) =>
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

        public static IEnumerable<ConstructorInfo> GetDeclaredConstructors(this Type type) =>
            type.GetConstructors(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type) =>
            type.GetMethods(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type type) =>
            type.GetProperties(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        public static bool IsPublic(this Type type) =>
            type.IsPublic;

        public static bool IsNestedPublic(this Type type) =>
            type.IsNestedPublic;

        public static bool IsGenericType(this Type type) =>
            type.IsGenericType;
#endif

        public static string GetFullName(this Type type) =>
            $"{type.Namespace}.{type.Name}";

        public static string GetFullName(this MemberInfo member) =>
            member switch
            {
#if !NET40
                TypeInfo typeInfo => GetFullName(typeInfo.AsType()),
#endif
#if !NETSTANDARD1_1
                Type type => GetFullName(type),
#endif
                ConstructorInfo constructor => GetFullName(member.DeclaringType!),
                _ => member.DeclaringType is Type declaringType ?
                    $"{GetFullName(declaringType)}.{member.Name}" :
                    member.Name
            };

        public static string GetReadableName(this Type type) =>
            SharpSymbols.ReadableTypeNames.TryGetValue(type, out var name) ?
                name :
                GetFullName(type);

        public static string GetReadableName(this MemberInfo member) =>
            member switch
            {
#if !NET40
                TypeInfo typeInfo => GetReadableName(typeInfo.AsType()),
#endif
#if !NETSTANDARD1_1
                Type type => GetReadableName(type),
#endif
                ConstructorInfo constructor => GetReadableName(member.DeclaringType!),
                _ => member.DeclaringType is Type declaringType ?
                    $"{GetReadableName(declaringType)}.{member.Name}" :
                    member.Name
            };
    }
}
