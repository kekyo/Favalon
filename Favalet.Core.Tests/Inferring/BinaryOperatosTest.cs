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

using Favalet.Contexts;
using Favalet.Expressions;
using NUnit.Framework;
using System;
using Favalet.Expressions.Algebraic;
using static Favalet.CLRGenerator;
using static Favalet.Generator;
using TextRange = Favalet.Ranges.TextRange;

namespace Favalet.Inferring
{
    [TestFixture]
    public sealed class BinaryOperatosTest
    {
        private static void AssertLogicalEqual(
            IExpression expression,
            IExpression expected,
            IExpression actual)
        {
            if (!ExpressionAssert.Equals(expected, actual))
            {
                Assert.Fail(
                    "Expression = {0}\r\nExpected   = {1}\r\nActual     = {2}",
                    expression.GetPrettyString(PrettyStringTypes.Readable),
                    expected.GetPrettyString(PrettyStringTypes.Readable),
                    actual.GetPrettyString(PrettyStringTypes.Readable));
            }
        }
        
        private static AndExpression And(IExpression lhs, IExpression rhs) =>
            AndExpression.Create(lhs, rhs, TextRange.Unknown);
        private static AndExpression And(IExpression lhs, IExpression rhs, IExpression higherOrder) =>
            AndExpression.UnsafeCreate(lhs, rhs, higherOrder, TextRange.Unknown);
        private static OrExpression Or(IExpression lhs, IExpression rhs) =>
            OrExpression.Create(lhs, rhs, TextRange.Unknown);
        private static OrExpression Or(IExpression lhs, IExpression rhs, IExpression higherOrder) =>
            OrExpression.UnsafeCreate(lhs, rhs, higherOrder, TextRange.Unknown);

        private static readonly Func<IExpression, IExpression, IExpression?, IExpression>[] BinaryOperators =
            new[]
            {
                new Func<IExpression, IExpression, IExpression?, IExpression>((lhs, rhs, ho) =>
                    ho is IExpression ? And(lhs, rhs, ho) : And(lhs, rhs)),
                new Func<IExpression, IExpression, IExpression?, IExpression>((lhs, rhs, ho) =>
                    ho is IExpression ? Or(lhs, rhs, ho) : Or(lhs, rhs)),
            };

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithoutAnnotation1(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // true && false
            var expression =
                oper(
                    Variable("true"),
                    Variable("false"),
                    null);

            var actual = environment.Infer(expression);

            // (true:'0 && false:'0):'0
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                oper(
                    Variable("true", ph0),
                    Variable("false", ph0),
                    ph0);

            AssertLogicalEqual(expression, expected, actual);
        }

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithoutAnnotation2(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // (true && false) && true
            var expression =
                And(
                    And(
                        Variable("true"),
                        Variable("false")),
                    Variable("true"));

            var actual = environment.Infer(expression);

            // (true:'0 && false:'0) && true:'0
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                And(
                    And(
                        Variable("true", ph0),
                        Variable("false", ph0),
                        ph0),
                    Variable("true", ph0),
                    ph0);

            AssertLogicalEqual(expression, expected, actual);
        }

        /////////////////////////////////////////////////////

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithAnnotation11(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // (true && false):bool
            var expression =
                oper(
                    Variable("true"),
                    Variable("false"),
                    Type<bool>());

            var actual = environment.Infer(expression);

            // (true:bool && false:bool):bool
            var expected =
                oper(
                    Variable("true", Type<bool>()),
                    Variable("false", Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithAnnotation12(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // true:bool && false
            var expression =
                oper(
                    Variable("true", Type<bool>()),
                    Variable("false"),
                    null);

            var actual = environment.Infer(expression);

            // (true:bool && false:bool):bool
            var expected =
                oper(
                    Variable("true", Type<bool>()),
                    Variable("false", Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithAnnotation13(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // true && false:bool
            var expression =
                oper(
                    Variable("true"),
                    Variable("false", Type<bool>()),
                    null);

            var actual = environment.Infer(expression);

            // (true:bool && false:bool):bool
            var expected =
                oper(
                    Variable("true", Type<bool>()),
                    Variable("false", Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithAnnotation14(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // true:bool && false:bool
            var expression =
                oper(
                    Variable("true", Type<bool>()),
                    Variable("false", Type<bool>()),
                    null);

            var actual = environment.Infer(expression);

            // (true:bool && false:bool):bool
            var expected =
                oper(
                    Variable("true", Type<bool>()),
                    Variable("false", Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithAnnotation15(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // (true:bool && false:bool):bool
            var expression =
                oper(
                    Variable("true", Type<bool>()),
                    Variable("false", Type<bool>()),
                    Type<bool>());

            var actual = environment.Infer(expression);

            // (true:bool && false:bool):bool
            var expected =
                oper(
                    Variable("true", Type<bool>()),
                    Variable("false", Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }

        /////////////////////////////////////////////////////

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithAnnotation21(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // (true && (false && true)):bool
            var expression =
                oper(
                    Variable("true"),
                    oper(
                        Variable("false"),
                        Variable("true"),
                        null),
                    Type<bool>());

            var actual = environment.Infer(expression);

            // (true:bool && (false:bool && true:bool):bool):bool
            var expected =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true", Type<bool>()),
                        Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithAnnotation22(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // true:bool && (false && true)
            var expression =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false"),
                        Variable("true"),
                        null),
                    null);

            var actual = environment.Infer(expression);

            // (true:bool && (false:bool && true:bool):bool):bool
            var expected =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true", Type<bool>()),
                        Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithAnnotation23(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // true && (false:bool && true)
            var expression =
                oper(
                    Variable("true"),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true"),
                        null),
                    null);

            var actual = environment.Infer(expression);

            // (true:bool && (false:bool && true:bool):bool):bool
            var expected =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true", Type<bool>()),
                        Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithAnnotation24(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // true && (false && true:bool)
            var expression =
                oper(
                    Variable("true"),
                    oper(
                        Variable("false"),
                        Variable("true", Type<bool>()),
                        null),
                    null);

            var actual = environment.Infer(expression);

            // (true:bool && (false:bool && true:bool):bool):bool
            var expected =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true", Type<bool>()),
                        Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithAnnotation25(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // true && (false && true):bool
            var expression =
                oper(
                    Variable("true"),
                    oper(
                        Variable("false"),
                        Variable("true"),
                        Type<bool>()),
                    null);

            var actual = environment.Infer(expression);

            // (true:bool && (false:bool && true:bool):bool):bool
            var expected =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true", Type<bool>()),
                        Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithAnnotation26(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // true:bool && (false:bool && true)
            var expression =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true"),
                        null),
                    null);

            var actual = environment.Infer(expression);

            // (true:bool && (false:bool && true:bool):bool):bool
            var expected =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true", Type<bool>()),
                        Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithAnnotation27(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // true && (false:bool && true:bool)
            var expression =
                oper(
                    Variable("true"),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true", Type<bool>()),
                        null),
                    null);

            var actual = environment.Infer(expression);

            // (true:bool && (false:bool && true:bool):bool):bool
            var expected =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true", Type<bool>()),
                        Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithAnnotation28(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // true:bool && (false && true:bool)
            var expression =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false"),
                        Variable("true", Type<bool>()),
                        null),
                    null);

            var actual = environment.Infer(expression);

            // (true:bool && (false:bool && true:bool):bool):bool
            var expected =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true", Type<bool>()),
                        Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithAnnotation29(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // true:bool && (false:bool && true:bool)
            var expression =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true", Type<bool>()),
                        null),
                    null);

            var actual = environment.Infer(expression);

            // (true:bool && (false:bool && true:bool):bool):bool
            var expected =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true", Type<bool>()),
                        Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithAnnotation30(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // true:bool && (false && true):bool
            var expression =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false"),
                        Variable("true"),
                        Type<bool>()),
                    null);

            var actual = environment.Infer(expression);

            // (true:bool && (false:bool && true:bool):bool):bool
            var expected =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true", Type<bool>()),
                        Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithAnnotation31(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // true:bool && (false:bool && true):bool
            var expression =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true"),
                        Type<bool>()),
                    null);

            var actual = environment.Infer(expression);

            // (true:bool && (false:bool && true:bool):bool):bool
            var expected =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true", Type<bool>()),
                        Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithAnnotation32(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // true:bool && (false && true:bool):bool
            var expression =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false"),
                        Variable("true", Type<bool>()),
                        Type<bool>()),
                    null);

            var actual = environment.Infer(expression);

            // (true:bool && (false:bool && true:bool):bool):bool
            var expected =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true", Type<bool>()),
                        Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithAnnotation33(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // true:bool && (false:bool && true:bool):bool
            var expression =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true", Type<bool>()),
                        Type<bool>()),
                    null);

            var actual = environment.Infer(expression);

            // (true:bool && (false:bool && true:bool):bool):bool
            var expected =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true", Type<bool>()),
                        Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }

        [TestCaseSource(nameof(BinaryOperators))]
        public void BinaryWithAnnotation34(
            Func<IExpression, IExpression, IExpression?, IExpression> oper)
        {
            var environment = CLREnvironments();

            // (true:bool && (false:bool && true:bool):bool):bool
            var expression =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true", Type<bool>()),
                        Type<bool>()),
                    Type<bool>());

            var actual = environment.Infer(expression);

            // (true:bool && (false:bool && true:bool):bool):bool
            var expected =
                oper(
                    Variable("true", Type<bool>()),
                    oper(
                        Variable("false", Type<bool>()),
                        Variable("true", Type<bool>()),
                        Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }
    }
}
