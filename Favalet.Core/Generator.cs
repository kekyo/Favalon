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
using Favalet.Expressions.Algebraic;
using Favalet.Expressions.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Favalet.Ranges;

namespace Favalet
{
    // TODO: range
    
    [DebuggerStepThrough]
    public static class Generator
    {
        public static Environments Environments() =>
            Favalet.Environments.Create();

        internal static readonly VariableTerm kind =
            VariableTerm.Create("*", FourthTerm.Instance, TextRange.Internal);

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static VariableTerm Kind() =>
            kind;
        public static VariableTerm Kind(string symbol) =>
            VariableTerm.Create(symbol, FourthTerm.Instance, TextRange.Unknown);

#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static UnspecifiedTerm Unspecified() =>
            UnspecifiedTerm.Instance;

        public static VariableTerm Variable(string symbol) =>
            VariableTerm.Create(symbol, TextRange.Unknown);
        public static VariableTerm Variable(string symbol, IExpression higherOrder) =>
            VariableTerm.Create(symbol, higherOrder, TextRange.Unknown);

        public static BoundVariableTerm BoundVariable(string symbol) =>
            BoundVariableTerm.Create(symbol, TextRange.Unknown);
        public static BoundVariableTerm BoundVariable(string symbol, IExpression higherOrder) =>
            BoundVariableTerm.Create(symbol, higherOrder, TextRange.Unknown);

        public static LogicalExpression Logical(IBinaryExpression operand) =>
            LogicalExpression.Create(operand, TextRange.Unknown);
#if !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static LogicalOperator Logical() =>
            LogicalOperator.Instance;

        public static AndExpression And(IExpression lhs, IExpression rhs) =>
            AndExpression.Create(lhs, rhs, TextRange.Unknown);
        public static IExpression? AndByExpressions(params IExpression[] expressions) =>
            LogicalCalculator.ConstructNested(
                expressions, AndExpression.Create, TextRange.Unknown);

        public static OrExpression Or(IExpression lhs, IExpression rhs) =>
            OrExpression.Create(lhs, rhs, TextRange.Unknown);
        public static IExpression? OrByExpressions(params IExpression[] expressions) =>
            LogicalCalculator.ConstructNested(
                expressions, OrExpression.Create, TextRange.Unknown);

        public static LambdaExpression Lambda(
            IExpression parameter, IExpression body) =>
            LambdaExpression.Create(parameter, body, TextRange.Unknown);
        public static LambdaExpression Lambda(
            string parameter, IExpression body) =>
            LambdaExpression.Create(BoundVariableTerm.Create(parameter, TextRange.Unknown), body, TextRange.Unknown);

        public static ApplyExpression Apply(
            IExpression function, IExpression argument) =>
            ApplyExpression.Create(function, argument, TextRange.Unknown);
    }
}
