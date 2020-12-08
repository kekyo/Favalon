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

using Favalet.Expressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Favalet.Internal
{
    [DebuggerStepThrough]
    internal sealed class ReferenceComparer :
        IEqualityComparer<IExpression>,
        IComparer<IExpression>
    {
        public bool Equals(IExpression? x, IExpression? y) =>
            object.ReferenceEquals(x, y);

        public int GetHashCode(IExpression? obj) =>
            RuntimeHelpers.GetHashCode(obj);

        public int Compare(IExpression? x, IExpression? y) =>
            RuntimeHelpers.GetHashCode(x).CompareTo(RuntimeHelpers.GetHashCode(y));

        public static readonly ReferenceComparer Instance =
            new ReferenceComparer();
    }
}
