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
using System.Reflection;

using static Favalet.CLRGenerator;
using static Favalet.Generator;

namespace Favalet.Reducing
{
    [TestFixture]
    public sealed class CallableTest
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

        [Test]
        public void LookupIdentity()
        {
            var environment = Environments();

            // let ABC = XYZ
            environment.MutableBind("ABC", Variable("XYZ"));

            // ABC
            var expression =
                Variable("ABC");

            var actual = environment.Reduce(expression);

            // XYZ
            var expected =
                Variable("XYZ");

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void PureLambda()
        {
            var environment = Environments();

            // arg -> arg && B
            var expression =
                Lambda(
                    "arg",
                    And(
                        Variable("arg"),
                        Variable("B")));

            var actual = environment.Reduce(expression);

            // arg -> arg && B
            var expected =
                Lambda(
                    "arg",
                    And(
                        Variable("arg"),
                        Variable("B")));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyLambda1()
        {
            var environment = Environments();

            // (arg -> arg && B) A
            var expression =
                Apply(
                    Lambda(
                        "arg",
                        And(
                            Variable("arg"),
                            Variable("B"))),
                    Variable("A"));

            var actual = environment.Reduce(expression);

            // A && B
            var expected =
                And(
                    Variable("A"),
                    Variable("B"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyLambda2()
        {
            var environment = Environments();

            // inner = arg1 -> arg1 && B
            environment.MutableBind(
                "inner",
                Lambda(
                    "arg1",
                    And(
                        Variable("arg1"),
                        Variable("B"))));

            // (arg2 -> inner arg2) A
            var expression =
                Apply(
                    Lambda(
                        "arg2",
                        Apply(
                            Variable("inner"),
                            Variable("arg2"))),
                    Variable("A"));

            var actual = environment.Reduce(expression);

            // A && B
            var expected =
                And(
                    Variable("A"),
                    Variable("B"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyLambda3()
        {
            var environment = Environments();

            // Same argument symbols.

            // inner = arg -> arg && B
            environment.MutableBind(
                "inner",
                Lambda(
                    "arg",
                    And(
                        Variable("arg"),
                        Variable("B"))));

            // (arg -> inner arg) A
            var expression =
                Apply(
                    Lambda(
                        "arg",
                        Apply(
                            Variable("inner"),
                            Variable("arg"))),
                    Variable("A"));

            var actual = environment.Reduce(expression);

            // A && B
            var expected =
                And(
                    Variable("A"),
                    Variable("B"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyNestedLambda1()
        {
            var environment = Environments();

            // Complex nested lambda (bind)

            // inner = arg1 -> arg2 -> arg2 && arg1
            environment.MutableBind(
                "inner",
                Lambda(
                    "arg1",
                    Lambda(
                        "arg2",
                        And(
                            Variable("arg2"),
                            Variable("arg1")))));

            // inner A B
            var expression =
                Apply(
                    Apply(
                        Variable("inner"),
                        Variable("A")),
                    Variable("B"));

            var actual = environment.Reduce(expression);

            // B && A
            var expected =
                And(
                    Variable("B"),
                    Variable("A"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyNestedLambda2()
        {
            var environment = Environments();

            // Complex nested lambda (bind)

            // inner = arg2 -> arg1 -> arg2 && arg1
            environment.MutableBind(
                "inner",
                Lambda(
                    "arg2",
                    Lambda(
                        "arg1",
                        And(
                            Variable("arg2"),
                            Variable("arg1")))));

            // inner A B
            var expression =
                Apply(
                    Apply(
                        Variable("inner"),
                        Variable("A")),
                    Variable("B"));

            var actual = environment.Reduce(expression);

            // A && B
            var expected =
                And(
                    Variable("A"),
                    Variable("B"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyLogicalOperator1()
        {
            var environment = Environments();

            // Logical (A && (B && A))
            var expression =
                Apply(
                    Logical(),
                    And(
                        Variable("A"),
                        And(
                            Variable("B"),
                            Variable("A"))));

            var actual = environment.Reduce(expression);

            // A && B
            var expected =
                And(
                    Variable("A"),
                    Variable("B"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyLogicalOperator2()
        {
            var environment = Environments();

            // logical = Logical
            environment.MutableBind(
                "logical",
                Logical());

            // logical (A && (B && A))
            var expression =
                Apply(
                    Variable("logical"),
                    And(
                        Variable("A"),
                        And(
                            Variable("B"),
                            Variable("A"))));

            var actual = environment.Reduce(expression);

            // A && B
            var expected =
                And(
                    Variable("A"),
                    Variable("B"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyMethod()
        {
            var environment = CLREnvironments();

            // Math.Sqrt pi
            var expression =
                Apply(
                    Method(typeof(Math).GetMethod("Sqrt")!),
                    Constant(Math.PI));

            var actual = environment.Reduce(expression);

            // [sqrt(pi)]
            var expected =
                Constant(Math.Sqrt(Math.PI));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyConstructor()
        {
            var environment = CLREnvironments();

            // Uri "http://example.com"
            var expression =
                Apply(
                    Method(typeof(Uri).GetConstructor(new[] { typeof(string) })!),
                    Constant("http://example.com"));

            var actual = environment.Reduce(expression);

            // [Uri("http://example.com")]
            var expected =
                Constant(new Uri("http://example.com"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [TestCase(123, "123")]
        [TestCase(123.456, "123.456")]
        public void ApplyOverloadedMethod(object argument, object result)
        {
            var environment = CLREnvironments();
            
            // Convert.ToString(123)
            var expression =
                Apply(
                    Variable("System.Convert.ToString"),
                    Constant(argument));

            var actual = environment.Reduce(expression);

            // "123"
            var expected =
                Constant(result);

            AssertLogicalEqual(expression, expected, actual);
        }
    }
}
