﻿/////////////////////////////////////////////////////////////////////////////////////////////////
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

        public static IEnumerable<Attribute> GetCustomAttributes(this Type type, Type attributeType, bool inherit) =>
            type.GetTypeInfo().GetCustomAttributes(attributeType, inherit);

        public static MethodInfo? GetGetMethod(this PropertyInfo property) =>
            property.GetMethod;

        public static MethodInfo? GetSetMethod(this PropertyInfo property) =>
            property.SetMethod;

        public static bool IsPublic(this Type type) =>
            type.GetTypeInfo().IsPublic;

        public static bool IsNestedPublic(this Type type) =>
            type.GetTypeInfo().IsNestedPublic;

        public static bool IsGenericType(this Type type) =>
            type.GetTypeInfo().IsGenericType;

        public static bool IsPrimitive(this Type type) =>
            type.GetTypeInfo().IsPrimitive;

        public static bool IsEnum(this Type type) =>
            type.GetTypeInfo().IsEnum;

        public static bool IsClass(this Type type) =>
            type.GetTypeInfo().IsClass;

        public static bool IsValueType(this Type type) =>
            type.GetTypeInfo().IsValueType;

        public static bool IsInterface(this Type type) =>
            type.GetTypeInfo().IsInterface;

        public static IEnumerable<Type> GetGenericArguments(this Type type) =>
            type.GenericTypeArguments;
        
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

        public static bool IsPrimitive(this Type type) =>
            type.IsPrimitive;

        public static bool IsEnum(this Type type) =>
            type.IsEnum;

        public static bool IsClass(this Type type) =>
            type.IsClass;

        public static bool IsValueType(this Type type) =>
            type.IsValueType;

        public static bool IsInterface(this Type type) =>
            type.IsInterface;
#endif

        public static string GetFullName(this Type type)
        {
            if (type.IsGenericType())
            {
                var ga = string.Join(".", type.GetGenericArguments().Select(GetFullName));
                var name = type.Name.Substring(0, type.Name.IndexOf('`'));
                return $"{type.Namespace}.{name}<{ga}>";
            }
            else
            {
                return $"{type.Namespace}.{type.Name}";
            }
        }

        public static string GetFullName(this MemberInfo member) =>
            member switch
            {
#if !NET40
                TypeInfo typeInfo => GetFullName(typeInfo.AsType()),
#endif
#if !NETSTANDARD1_1
                Type type => GetFullName(type),
#endif
                ConstructorInfo _ => GetFullName(member.DeclaringType!),
                _ => member.DeclaringType is { } declaringType ?
                    $"{GetFullName(declaringType)}.{member.Name}" :
                    member.Name
            };

        private static string? GetAliasName(Type type) =>
            type.GetCustomAttributes(typeof(AliasNameAttribute), true) is IEnumerable<AliasNameAttribute> names ?
                names.Select(name => name.Name).FirstOrDefault() : null;
        private static string? GetAliasName(MemberInfo member) =>
            member.GetCustomAttributes(typeof(AliasNameAttribute), true) is IEnumerable<AliasNameAttribute> names ?
                names.Select(name => name.Name).FirstOrDefault() : null;

        public static string GetReadableName(this Type type)
        {
            if (SharpSymbols.ReadableTypeNames.TryGetValue(type, out var readableName))
            {
                return readableName;
            }

            if (GetAliasName(type) is string aliasName)
            {
                return aliasName;
            }

            if (type.IsGenericType())
            {
                var ga = string.Join(".", type.GetGenericArguments().Select(GetReadableName));
                var name = type.Name.Substring(0, type.Name.IndexOf('`'));
                return $"{type.Namespace}.{name}<{ga}>";
            }
            else
            {
                return $"{type.Namespace}.{type.Name}";
            }
        }

        public static string GetReadableName(this MemberInfo member) =>
            member switch
            {
#if !NET40
                TypeInfo typeInfo => GetReadableName(typeInfo.AsType()),
#endif
#if !NETSTANDARD1_1
                Type type => GetReadableName(type),
#endif
                ConstructorInfo _ => GetReadableName(member.DeclaringType!),
                _ => member.DeclaringType is Type declaringType ?
                    (GetAliasName(member) is string aliasName ?
                        aliasName :
                        $"{GetReadableName(declaringType)}.{member.Name}") :
                    member.Name
            };
    }
}
