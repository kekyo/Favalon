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
using System;
using System.Diagnostics;
using System.Reflection;

namespace Favalet
{
    // TODO: range
    
    [DebuggerStepThrough]
    public static class CLRGenerator
    {
        public static Environments CLREnvironment(
#if DEBUG
            bool saveLastTopology = true
#else
            bool saveLastTopology = false
#endif
            ) =>
            Favalet.Environments.Create(CLRTypeCalculator.Instance, saveLastTopology);

        public static ITerm Type<T>() =>
            TypeTerm.From(typeof(T), TextRange.From(typeof(T)));
        public static ITerm Type(Type runtimeType) =>
            TypeTerm.From(runtimeType, TextRange.From(runtimeType));

        public static MethodTerm Method(MethodBase runtimeMethod) =>
            MethodTerm.From(runtimeMethod, TextRange.From(runtimeMethod));

        public static ConstantTerm Constant(object value) =>
            ConstantTerm.From(value, TextRange.Unknown);
    }
}
