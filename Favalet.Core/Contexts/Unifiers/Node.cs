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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Favalet.Contexts.Unifiers
{
    [DebuggerStepThrough]
    internal sealed class Node
    {
#if DEBUG
        public readonly IPlaceholderTerm Placeholder;
#endif
        public readonly HashSet<Unification> Unifications = new HashSet<Unification>();

        private Node(IPlaceholderTerm placeholder)
#if DEBUG
            => this.Placeholder = placeholder;
#else
            { }
#endif
            
        public override string ToString() =>
#if DEBUG
            $"{this.Placeholder.GetPrettyString(PrettyStringTypes.Minimum)} [" + 
            StringUtilities.Join(
                ",",
                this.Unifications.Select(unification => unification.ToString())) + "]";
#else
            $"[" + StringUtilities.Join(
                ",",
                this.Unifications.Select(unification => unification.ToString())) + "]";
#endif

        public static Node Create(IPlaceholderTerm placeholder) =>
            new Node(placeholder);
    }
}
