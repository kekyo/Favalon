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
using Favalet.Ranges;
using Favalet.Internal;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Favalet
{
    // TODO: range
    
    [DebuggerStepThrough]
    public static class CLRGenerator
    {
        public static CLREnvironments CLREnvironments(
#if DEBUG
            bool saveLastTopology = true
#else
            bool saveLastTopology = false
#endif
            ) =>
            Favalet.CLREnvironments.Create(saveLastTopology);

        public static TextRange TextRange(Assembly assembly) =>
            Favalet.Ranges.TextRange.Create(new Uri(assembly.GetName().Name!, UriKind.RelativeOrAbsolute), TextPosition.Zero);
        public static TextRange TextRange(MemberInfo member) =>
            TextRange(member.Module.Assembly);
#if NETSTANDARD1_1
        public static TextRange TextRange(Type type) =>
            TextRange(type.GetTypeInfo().Assembly);
#endif

        public static ITerm Type<T>() =>
            TypeTerm.From(typeof(T), TextRange(typeof(T)));
        public static ITerm Type(Type runtimeType) =>
            TypeTerm.From(runtimeType, TextRange(runtimeType));

        public static IExpression Method(MethodBase runtimeMethod) =>
            MethodTerm.From(runtimeMethod, TextRange(runtimeMethod));
        public static IExpression Method(Delegate d) =>
            MethodTerm.From(d, TextRange(d.GetMethodInfo()));

        public static IExpression Property(PropertyInfo runtimeProperty) =>
            PropertyTerm.From(runtimeProperty, TextRange(runtimeProperty));

        public static ConstantTerm Constant(object value) =>
            ConstantTerm.From(value, Favalet.Ranges.TextRange.Unknown);
    }
}
