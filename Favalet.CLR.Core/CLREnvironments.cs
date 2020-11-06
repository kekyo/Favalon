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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Favalet.Expressions;
using Favalet.Expressions.Specialized;
using Favalet.Internal;
using Favalet.Ranges;

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

            var assembly = typeof(object).GetAssembly();
            environments.MutableBindMembers(assembly);

            return environments;
        }
    }

    public static class CLREnvironmentsExtension
    {
        private static void MutableBindMember(IEnvironments environments, PropertyInfo property, TextRange range)
        {
            var propertyTerm = PropertyTerm.From(property, range);
            environments.MutableBind(
                BoundVariableTerm.Create(property.GetReadableName(), propertyTerm.HigherOrder, range),
                propertyTerm);
        }
        
        public static void MutableBindMember(this IEnvironments environments, PropertyInfo property) =>
            MutableBindMember(environments, property, TextRange.From(property));

        private static void MutableBindMember(IEnvironments environments, MethodInfo method, TextRange range)
        {
            var methodTerm = MethodTerm.From(method, range);
            environments.MutableBind(
                BoundVariableTerm.Create(method.GetReadableName(), methodTerm.HigherOrder, range),
                methodTerm);
        }
        
        public static void MutableBindMember(this IEnvironments environments, MethodInfo method) =>
            MutableBindMember(environments, method, TextRange.From(method));

        private static void MutableBindMembers(IEnvironments environments, Type type, TextRange range)
        {
            var typeTerm = TypeTerm.From(type, range);
            environments.MutableBind(
                BoundVariableTerm.Create(type.GetReadableName(), typeTerm.HigherOrder, range),
                typeTerm);

            var properties = type.GetProperties().
                Where(property =>
                    property.CanRead &&
                    (property.GetGetMethod() != null) &&
                    property.GetIndexParameters().Length == 0).
                ToDictionary(property => property.GetGetMethod());

            foreach (var property in properties.Values)
            {
                MutableBindMember(environments, property, range);
            }
                
            foreach (var method in type.GetMethods().
                Where(method =>
                    method.IsPublic && method.IsStatic && !method.IsGenericMethod &&
                    (method.ReturnType != typeof(void)) &&    // TODO: void
                    (method.GetParameters().Length == 1) &&   // TODO: 1parameter
                    !properties.ContainsKey(method)))
            {
                MutableBindMember(environments, method, range);
            }
        }

        public static void MutableBindMembers(this IEnvironments environments, Type type) =>
            MutableBindMembers(environments, type, TextRange.From(type));

        public static void MutableBindMembers(this IEnvironments environments, Assembly assembly)
        {
            var range = TextRange.From(assembly);
            foreach (var type in assembly.GetTypes().
                Where(type => type.IsPublic() && !type.IsNestedPublic() && !type.IsGenericType()))
            {
                MutableBindMembers(environments, type, range);
            }
        }
    }
}
