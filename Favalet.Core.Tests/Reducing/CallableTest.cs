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

using Favalet.Contexts;
using Favalet.Expressions;
using NUnit.Framework;
using System;

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

        public sealed class TypeConstructorTest<T>
        {
            private readonly T value;

            public TypeConstructorTest(T value) =>
                this.value = value;
            
            public string Foo(T value2) =>
                $"{this.value}_{value2}";
        }

        [Test]
        public void ApplyTypeConstructor1()
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindMembers(typeof(TypeConstructorTest<>));

            // TypeConstructorTest int
            var expression =
                Apply(
                    Variable("Favalet.Reducing.TypeConstructorTest"),
                    Type<int>());

            var actual = environments.Reduce(expression);

            // TypeConstructorTest<int>
            var expected =
                Type<TypeConstructorTest<int>>();

            AssertLogicalEqual(expression, expected, actual);
        }

        #region Overloads
        public static class OverloadTest1
        {
            public static string Overload(int value) =>
                value.ToString();
            public static string Overload(double value) =>
                value.ToString();
            public static string Overload(string value) =>
                value.ToString();
        }

        [TestCase(123, "123")]
        [TestCase(123.456, "123.456")]
        [TestCase("abc", "abc")]
        public void ApplyOverloadedMethod1(object argument, object result)
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindMembers(typeof(OverloadTest1));
            
            // OverloadTest1.Overload(123)
            var expression =
                Apply(
                    Variable("Favalet.Reducing.OverloadTest1.Overload"),
                    Constant(argument));

            var actual = environments.Reduce(expression);

            // "123"
            var expected =
                Constant(result);

            AssertLogicalEqual(expression, expected, actual);
        }
        
        public static class OverloadTest2
        {
            public static int Overload(int a, int b) =>
                a + b;
            public static double Overload(double a, double b) =>
                a + b;
            public static string Overload(string a, string b) =>
                a + b;
        }

        [TestCase(1, 2, 3)]
        [TestCase(1.0, 2.0, 3.0)]
        [TestCase("a", "b", "ab")]
        public void ApplyOverloadedMethod2(object a, object b, object r)
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindMembers(typeof(OverloadTest2));
            
            // OverloadTest2.Overload(1, 2)
            var expression =
                Apply(
                    Apply(
                        Variable("Favalet.Reducing.OverloadTest2.Overload"),
                        Constant(a)),
                    Constant(b));

            var actual = environments.Reduce(expression);

            // 3
            var expected =
                Constant(r);

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion

        /*
        public readonly struct OperatorTest
        {
            public readonly int v;

            public OperatorTest(int v) =>
                this.v = v;
            
            public static OperatorTest operator +(OperatorTest b) =>
                new OperatorTest(100 + b.v);
            public static OperatorTest operator -(OperatorTest b) =>
                new OperatorTest(100 - b.v);
        }

        [TestCase(1, 101)]
        public void Operator(int a, int r)
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindMembers(typeof(OperatorTest));
            
            // OperatorTest + 1
            var expression =
                Apply(
                    Variable("+"),
                    Constant(new OperatorTest(a)));

            var actual = environments.Reduce(expression);

            // "101"
            var expected =
                Constant(new OperatorTest(r));

            AssertLogicalEqual(expression, expected, actual);
        }
    */
    }
}
