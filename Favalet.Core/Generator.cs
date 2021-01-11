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

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static TypeKindTerm Kind() =>
            TypeKindTerm.Instance;
        public static VariableTerm Kind(string symbol) =>
            VariableTerm.Create(symbol, FourthTerm.Instance, TextRange.Unknown);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static UnspecifiedTerm Unspecified() =>
            UnspecifiedTerm.Instance;

        public static VariableTerm Variable(string symbol) =>
            VariableTerm.Create(symbol, TextRange.Unknown);
        public static VariableTerm Variable(string symbol, IExpression higherOrder) =>
            VariableTerm.Create(symbol, higherOrder, TextRange.Unknown);

        public static BoundVariableTerm BoundVariable(string symbol) =>
            BoundVariableTerm.Create(symbol, BoundAttributes.Neutral, TextRange.Unknown);
        public static BoundVariableTerm BoundVariable(string symbol, BoundAttributes attributes) =>
            BoundVariableTerm.Create(symbol, attributes, TextRange.Unknown);
        public static BoundVariableTerm BoundVariable(string symbol, IExpression higherOrder) =>
            BoundVariableTerm.Create(symbol, BoundAttributes.Neutral, higherOrder, TextRange.Unknown);
        public static BoundVariableTerm BoundVariable(string symbol, BoundAttributes attributes, IExpression higherOrder) =>
            BoundVariableTerm.Create(symbol, attributes, higherOrder, TextRange.Unknown);

        public static LogicalExpression Logical(IBinaryExpression operand) =>
            LogicalExpression.Create(operand, TextRange.Unknown);
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
            LambdaExpression.Create(
                BoundVariableTerm.Create(parameter, BoundAttributes.Neutral, TextRange.Unknown),
                body,
                TextRange.Unknown);
        public static LambdaExpression Lambda(
            string parameter, BoundAttributes attributes, IExpression body) =>
            LambdaExpression.Create(
                BoundVariableTerm.Create(parameter, attributes, TextRange.Unknown),
                body,
                TextRange.Unknown);

        public static ApplyExpression Apply(
            IExpression function, IExpression argument) =>
            ApplyExpression.Create(function, argument, TextRange.Unknown);

        public static BoundAttributes Neutral() =>
            BoundAttributes.Neutral;
        public static BoundAttributes PrefixLeftToRight(BoundPrecedences precedence = BoundPrecedences.Neutral) =>
            BoundAttributes.PrefixLeftToRight(precedence);
        public static BoundAttributes InfixLeftToRight(BoundPrecedences precedence = BoundPrecedences.Neutral) =>
            BoundAttributes.InfixLeftToRight(precedence);
        public static BoundAttributes PrefixRightToLeft(BoundPrecedences precedence = BoundPrecedences.Neutral) =>
            BoundAttributes.PrefixRightToLeft(precedence);
        public static BoundAttributes InfixRightToLeft(BoundPrecedences precedence = BoundPrecedences.Neutral) =>
            BoundAttributes.InfixRightToLeft(precedence);
        public static BoundAttributes PrefixLeftToRight(int precedence) =>
            BoundAttributes.PrefixLeftToRight(precedence);
        public static BoundAttributes InfixLeftToRight(int precedence) =>
            BoundAttributes.InfixLeftToRight(precedence);
        public static BoundAttributes PrefixRightToLeft(int precedence) =>
            BoundAttributes.PrefixRightToLeft(precedence);
        public static BoundAttributes InfixRightToLeft(int precedence) =>
            BoundAttributes.InfixRightToLeft(precedence);
    }
}
