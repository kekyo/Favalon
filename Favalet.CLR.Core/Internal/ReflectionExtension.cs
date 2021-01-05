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

        public static bool IsGenericTypeDefinition(this Type type) =>
            type.GetTypeInfo().IsGenericTypeDefinition;

        public static bool IsGenericParameter(this Type type) =>
            type.IsGenericParameter;

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
        
        public static bool IsGenericTypeDefinition(this Type type) =>
            type.IsGenericTypeDefinition;

        public static bool IsGenericParameter(this Type type) =>
            type.IsGenericParameter;

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

        public static string GetFullName(this Type type, bool includeGenericArguments = false)
        {
            if (type.IsGenericParameter())
            {
                return type.Name;
            }
            
            if (type.IsGenericType())
            {
                var name = type.Name.Substring(0, type.Name.IndexOf('`'));
                if (includeGenericArguments)
                {
                    var ga = StringUtilities.Join(
                        ".", type.GetGenericArguments().Select(gt => GetFullName(gt, includeGenericArguments)));
                    return $"{type.Namespace}.{name}<{ga}>";
                }
                else
                {
                    return $"{type.Namespace}.{name}";
                }
            }
            else
            {
                return $"{type.Namespace}.{type.Name}";
            }
        }

        public static string GetFullName(this MemberInfo member, bool includeGenericArguments = false)
        {
            switch (member)
            {
#if !NET35 && !NET40
                case TypeInfo typeInfo:
                    return GetFullName(typeInfo.AsType(), includeGenericArguments);
#endif
#if !NETSTANDARD1_1
                case Type type:
                    return GetFullName(type, includeGenericArguments);
#endif
                case ConstructorInfo _:
                    return GetFullName(member.DeclaringType!, includeGenericArguments);
                default:
                    if (member is MethodInfo method && method.IsGenericMethod)
                    {
                        var name = method.Name.Substring(0, method.Name.IndexOf('`'));
                        if (includeGenericArguments)
                        {
                            var ga = StringUtilities.Join(
                                ".", method.GetGenericArguments().Select(gt => GetFullName(gt, includeGenericArguments)));
                            return method.DeclaringType is { } declaringType ?
                                $"{GetFullName(declaringType, includeGenericArguments)}.{name}<{ga}>" :
                                name;
                        }
                        else
                        {
                            return method.DeclaringType is { } declaringType ?
                                $"{GetFullName(declaringType, includeGenericArguments)}.{name}" :
                                name;
                        }
                    }
                    else
                    {
                        return member.DeclaringType is { } declaringType ?
                            $"{GetFullName(declaringType, includeGenericArguments)}.{member.Name}" :
                            member.Name;
                    }
            }
        }

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

            if (GetAliasName(type) is { } aliasName)
            {
                return aliasName;
            }

            if (type.IsGenericParameter())
            {
                return type.Name;
            }
            
            if (type.IsGenericType())
            {
                var name = type.Name.Substring(0, type.Name.IndexOf('`'));
                var ga = StringUtilities.Join(".", type.GetGenericArguments().Select(GetReadableName));
                return $"{type.Namespace}.{name}<{ga}>";
            }
            else
            {
                return $"{type.Namespace}.{type.Name}";
            }
        }

        public static string GetReadableName(this MemberInfo member)
        {
            switch (member)
            {
#if !NET35 && !NET40
                case TypeInfo typeInfo:
                    return GetReadableName(typeInfo.AsType());
#endif
#if !NETSTANDARD1_1
                case Type type:
                    return GetReadableName(type);
#endif
                case ConstructorInfo _:
                    return GetReadableName(member.DeclaringType!);
                default:
                    if (member is MethodInfo method && method.IsGenericMethod)
                    {
                        var name = member.Name.Substring(0, member.Name.IndexOf('`'));
                        var ga = StringUtilities.Join(
                            ".", method.GetGenericArguments().Select(GetReadableName));
                        return member.DeclaringType is { } declaringType ?
                            (GetAliasName(member) is { } aliasName ?
                                aliasName :
                                $"{GetReadableName(declaringType)}.{name}<{ga}>") :
                            name;
                    }
                    else
                    {
                        return member.DeclaringType is { } declaringType ?
                            (GetAliasName(member) is { } aliasName ?
                                aliasName :
                                $"{GetReadableName(declaringType)}.{member.Name}") :
                            member.Name;
                    }
            }
        }

#if NET35 || NET40
        public static MethodInfo GetMethodInfo(this Delegate d) =>
            d.Method;
#endif
    }
}
