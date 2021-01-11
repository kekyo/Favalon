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
using System;

namespace Favalet
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class AliasNameAttribute : Attribute
    {
        public readonly string Name;
        public readonly BoundAttributes Attributes;

        public AliasNameAttribute(string name)
        {
            this.Name = name;
            this.Attributes = BoundAttributes.Neutral;
        }

        public AliasNameAttribute(
            string name,
            BoundPositions position,
            BoundAssociativities associativity,
            BoundPrecedences precedence)
        {
            this.Name = name;
            this.Attributes = BoundAttributes.Create(position, associativity, precedence);
        }

        public AliasNameAttribute(
            string name,
            BoundPositions position,
            BoundAssociativities associativity,
            int precedence)
        {
            this.Name = name;
            this.Attributes = BoundAttributes.Create(position, associativity, precedence);
        }
    }
}
