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
using System.Diagnostics;
using System.Runtime.InteropServices;
using Favalet.Expressions;

namespace Favalet.Contexts.Unifiers
{
    internal enum UnificationPolarities
    {
        // Indicate "==>", forward polarity, will make covariant (widen)
        Out,
        // Indicate "<==", backward polarity, will make contravariant (narrow)
        In,
        // Both direction "<=>"  
        Both
    }

    [DebuggerStepThrough]
    internal sealed class Unification :
        IEquatable<Unification?>
    {
        public IExpression Expression { get; private set; }
        public readonly UnificationPolarities Polarity;

        private Unification(
            IExpression expression,
            UnificationPolarities polarity)
        {
            this.Expression = expression;
            this.Polarity = polarity;
        }

        public void UpdateExpression(IExpression expression) =>
            this.Expression = expression;

        public override int GetHashCode() =>
            this.Expression.GetHashCode() ^
            this.Polarity.GetHashCode();
        
        public bool Equals(Unification? rhs) =>
            rhs != null &&
            this.Expression.Equals(rhs.Expression) &&
            (this.Polarity == rhs.Polarity);

        public string ToString(PrettyStringTypes type)
        {
            var polarity = this.Polarity switch
            {
                UnificationPolarities.Out => "==>",
                UnificationPolarities.In => "<==",
                _ => "<=>"
            };
            return $"{polarity} {this.Expression.GetPrettyString(type)}";
        }
        public override string ToString() =>
            this.ToString(PrettyStringTypes.Readable);

        public static Unification Create(
            IExpression expression,
            UnificationPolarities polarity) =>
            new Unification(expression, polarity);
    }
}
