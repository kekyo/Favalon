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

using Favalet.Expressions;
using Favalet.Expressions.Specialized;
using Favalet.Internal;
using Favalet.Ranges;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Favalet
{
    public sealed class CLREnvironments : Environments
    {
        [DebuggerStepThrough]
        private CLREnvironments(bool saveLastTopology) :
            base(CLRTypeCalculator.Instance, saveLastTopology)
        { }

        [DebuggerStepThrough]
        public new static CLREnvironments Create(
#if DEBUG
            bool saveLastTopology = true
#else
            bool saveLastTopology = false
#endif
        )
        {
            var environments = new CLREnvironments(saveLastTopology);
            environments.MutableBindCLRDefaults();
            return environments;
        }
    }

    public static class CLREnvironmentsExtension
    {
        private static void MutableBind(
            IEnvironments environments, IBoundVariableTerm bound, IExpression expression)
        {
            if (environments is Environments re)
            {
                re.MutableBind(BoundAttributes.PrefixLeftToRight, bound, expression, false);
            }
            else
            {
                environments.MutableBind(BoundAttributes.PrefixLeftToRight, bound, expression);
            }
        }
        
        [DebuggerStepThrough]
        private static void MutableBindMembersByAliasNames(
            IEnvironments environments,
            MemberInfo member,
            IExpression expression,
            TextRange range)
        {
            if (member.GetCustomAttributes(typeof(AliasNameAttribute), true) is AliasNameAttribute[] names)
            {
                foreach (var name in names)
                {
                    MutableBind(
                        environments,
                        BoundVariableTerm.Create(name.Name, expression.HigherOrder, range),
                        expression);
                }
            }
        }
        
        private static ITerm MutableBindMember(
            IEnvironments environments, PropertyInfo property, TextRange range)
        {
            var propertyTerm = PropertyTerm.From(property, range);
            
            var isStatic =
                (property.GetGetMethod() ?? property.GetSetMethod())?.IsStatic ?? true;
            var name = isStatic ?
                property.GetFullName() :
                property.Name;
            
            MutableBind(
                environments,
                BoundVariableTerm.Create(
                    name,
                    propertyTerm.HigherOrder,
                    range),
                propertyTerm);

            MutableBindMembersByAliasNames(environments, property, propertyTerm, range);
            
            return propertyTerm;
        }
        
        public static ITerm MutableBindMember(
            this IEnvironments environments, PropertyInfo property) =>
            MutableBindMember(environments, property, CLRGenerator.TextRange(property));

        private static ITerm MutableBindMember(
            IEnvironments environments, MethodBase method, TextRange range)
        {
            var methodTerm = MethodTerm.From(method, range);

            var name =
                method is ConstructorInfo ?
                    method.DeclaringType!.GetFullName() :
                    (method.IsStatic ? method.GetFullName() : method.Name);
            
            MutableBind(
                environments,
                BoundVariableTerm.Create(
                    name,
                    methodTerm.HigherOrder,
                    range),
                methodTerm);
            
            if (SharpSymbols.OperatorSymbols.TryGetValue(method.Name, out var symbol))
            {
                MutableBind(
                    environments,
                    BoundVariableTerm.Create(symbol, methodTerm.HigherOrder, range),
                    methodTerm);
            }

            MutableBindMembersByAliasNames(environments, method, methodTerm, range);
            
            return methodTerm;
        }
        
        public static ITerm MutableBindMember(
            this IEnvironments environments, MethodBase method) =>
            MutableBindMember(environments, method, CLRGenerator.TextRange(method));

        private static ITerm MutableBindMember(
            IEnvironments environments, Type type, TextRange range)
        {
            var typeTerm = TypeTerm.From(type, range);
            MutableBind(
                environments,
                BoundVariableTerm.Create(type.GetFullName(), typeTerm.HigherOrder, range),
                typeTerm);

            if (SharpSymbols.ReadableTypeNames.TryGetValue(type, out var name))
            {
                MutableBind(
                    environments,
                    BoundVariableTerm.Create(name, typeTerm.HigherOrder, range),
                    typeTerm);
            }
            
            return typeTerm;
        }

        public static ITerm MutableBindMember(
            this IEnvironments environments, Type type) =>
            MutableBindMember(environments, type, CLRGenerator.TextRange(type));

        private static ITerm MutableBindMembers(
            IEnvironments environments, Type type, TextRange range)
        {
            var typeTerm = MutableBindMember(environments, type, range);
                
            foreach (var constructor in type.GetDeclaredConstructors().
                Where(constructor =>
                    constructor.IsPublic && !constructor.IsStatic &&
                    (constructor.GetParameters().Length == 1)))  // TODO: 1parameter
            {
                MutableBindMember(environments, constructor, range);
            }

            var properties = type.GetDeclaredProperties().
                Where(property =>
                    property.CanRead &&   // TODO: readonly
                    property.GetGetMethod() is { } method &&
                    property.GetIndexParameters().Length == 0).
                ToDictionary(property => property.GetGetMethod()!);

            foreach (var property in properties.Values)
            {
                MutableBindMember(environments, property, range);
            }
                
            foreach (var method in type.GetDeclaredMethods().
                Where(method =>
                    method.IsPublic && !method.IsGenericMethod &&
                    (method.ReturnType != typeof(void)) &&    // TODO: void
                    (method.GetParameters().Length == (method.IsStatic ? 1 : 0)) &&   // TODO: 1parameter
                    !properties.ContainsKey(method)))
            {
                MutableBindMember(environments, method, range);
            }

            return typeTerm;
        }

        public static ITerm MutableBindMembers(
            this IEnvironments environments, Type type) =>
            MutableBindMembers(environments, type, CLRGenerator.TextRange(type));

        public static void MutableBindMembers(
            this IEnvironments environments, Assembly assembly)
        {
            var range = CLRGenerator.TextRange(assembly);
            foreach (var type in assembly.GetTypes().
                Where(type => type.IsPublic() && !type.IsNestedPublic() && !type.IsGenericType()))
            {
                MutableBindMembers(environments, type, range);
            }
        }
        
        [DebuggerStepThrough]
        public static void MutableBindCLRDefaults(
            this IEnvironments environments)
        {
            // Bind default assemblies.
            foreach (var assembly in new[] {
                    typeof(object), typeof(Uri), typeof(Enumerable)
                }.
                Select(type => type.GetAssembly()).
                Distinct())
            {
                MutableBindMembers(environments, assembly);
            }
        }
    }
}
