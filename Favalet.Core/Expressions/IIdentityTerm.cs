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

namespace Favalet.Expressions
{
    public interface IIdentityTerm :
        ITerm
    {
        string Symbol { get; }
        
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object Identity { get; }
    }

    [DebuggerStepThrough]
    public static class IdentityTermExtension
    {
        public static void Deconstruct(
            this IIdentityTerm identity,
            out string symbol) =>
            symbol = identity.Symbol;
    }
    
    [DebuggerStepThrough]
    public sealed class IdentityTermComparer :
        IEqualityComparer<IIdentityTerm>,
        IComparer<IIdentityTerm>
    {
        private IdentityTermComparer()
        { }

        public bool Equals(IIdentityTerm x, IIdentityTerm y) =>
            x.Identity.Equals(y.Identity);

        public int GetHashCode(IIdentityTerm obj) =>
            obj.Identity.GetHashCode();

        public int Compare(IIdentityTerm x, IIdentityTerm y) =>
            (x.Identity, y.Identity) switch
            {
                (int x_, int y_) => x_.CompareTo(y_),
                (string x_, string y_) => string.Compare(x_, y_, StringComparison.Ordinal),
                (int _, string _) => -1,
                (string _, int _) => 1,
                _ => string.Compare(
                    x.Identity.ToString(),
                    y.Identity.ToString(),
                    StringComparison.Ordinal)
            };

        public static readonly IdentityTermComparer Instance =
            new IdentityTermComparer();
    }
}
