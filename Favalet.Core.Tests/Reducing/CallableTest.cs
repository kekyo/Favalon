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
using System.Text;
using Favalet.Expressions.Specialized;
using NUnit.Framework.Interfaces;
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

        #region Lambda
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
        #endregion

        #region Logical operator
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
        #endregion

        #region Methods
        public sealed class StaticMethodTest
        {
            public static double Method(string value) =>
                double.Parse(value);
            public static void VoidMethod(StringBuilder sb, int value) =>
                sb.Append($"abc{value}");
        }

        [Test]
        public void ApplyStaticMethod1()
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers<StaticMethodTest>();

            // StaticMethodTest.Method "123.456"
            var expression =
                Apply(
                    Variable("Favalet.Reducing.StaticMethodTest.Method"),
                    Constant("123.456"));

            var actual = environments.Reduce(expression);

            // 123.456
            var expected =
                Constant(123.456);

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyStaticMethod2()
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers<StaticMethodTest>();

            // StaticMethodTest.VoidMethod sb 123
            var sb = new StringBuilder();
            var expression =
                Apply(
                    Apply(
                        Variable("Favalet.Reducing.StaticMethodTest.VoidMethod"),
                        Constant(sb)),
                    Constant(123));

            var actual = environments.Reduce(expression);

            // ()
            var expected =
                Unit();

            AssertLogicalEqual(expression, expected, actual);
            Assert.AreEqual(sb.ToString(), "abc123");
        }

        [Test]
        public void ApplyExtensionMethod1()
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers(typeof(ExtensionMethodTest));
            
            // 1.Method(2, 3)
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Variable("Favalet.Reducing.ExtensionMethodTest.Method"),
                            Constant(2)),
                        Constant(3)),
                    Constant(1));

            var actual = environments.Reduce(expression);

            // 321
            var expected =
                Constant(321);

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyExtensionMethod2()
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers(typeof(ExtensionMethodTest));
            
            // sb.VoidMethod(2, 3)
            var sb = new StringBuilder();
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Variable("Favalet.Reducing.ExtensionMethodTest.VoidMethod"),
                            Constant(2)),
                        Constant(3)),
                    Constant(sb));

            var actual = environments.Reduce(expression);

            // ()
            var expected =
                Unit();

            AssertLogicalEqual(expression, expected, actual);
            Assert.AreEqual("23", sb.ToString());
        }

        public sealed class InstanceMethodTest
        {
            public readonly StringBuilder fv;
            public InstanceMethodTest(string fv) =>
                this.fv = new StringBuilder(fv);
            public string Overload() =>
                this.fv.ToString();
            public string Overload(int value) =>
                this.fv.ToString() + value.ToString();
            public string Overload(int value1, double value2) =>
                this.fv.ToString() + value1.ToString() + "_" + value2.ToString();
            public void VoidOverload() =>
                this.fv.Append("abc");
            public void VoidOverload(int value) =>
                this.fv.Append("abc" + value.ToString());
            public void VoidOverload(int value1, double value2) =>
                this.fv.Append("abc" + value1.ToString() + "_" + value2.ToString());
        }

        [Test]
        public void ApplyInstanceMethod1()
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers<InstanceMethodTest>();
            
            // Overload instance
            var instance = new InstanceMethodTest("aaa");
            var expression =
                Apply(
                    Variable("Overload"),
                    Constant(instance));

            var actual = environments.Reduce(expression);

            // "aaa"
            var expected =
                Constant("aaa");

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyInstanceMethod2()
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers<InstanceMethodTest>();
            
            // Overload 123 instance
            var instance = new InstanceMethodTest("aaa");
            var expression =
                Apply(
                    Apply(
                        Variable("Overload"),
                        Constant(123)),
                    Constant(instance));

            var actual = environments.Reduce(expression);

            // "aaa123"
            var expected =
                Constant("aaa123");

            AssertLogicalEqual(expression, expected, actual);
        }
   
        [Test]
        public void ApplyInstanceMethod3()
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers<InstanceMethodTest>();
            
            // Overload 123 123.456 instance
            var instance = new InstanceMethodTest("aaa");
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Variable("Overload"),
                            Constant(123)),
                        Constant(123.456)),
                    Constant(instance));

            var actual = environments.Reduce(expression);

            // "aaa123_123.456"
            var expected =
                Constant("aaa123_123.456");

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyInstanceMethod4()
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers<InstanceMethodTest>();
            
            // VoidOverload instance
            var instance = new InstanceMethodTest("aaa");
            var expression =
                Apply(
                    Variable("VoidOverload"),
                    Constant(instance));

            var actual = environments.Reduce(expression);

            // ()
            var expected =
                Unit();

            AssertLogicalEqual(expression, expected, actual);
            Assert.AreEqual("aaaabc", instance.fv.ToString());
        }

        [Test]
        public void ApplyInstanceMethod5()
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers<InstanceMethodTest>();
            
            // VoidOverload 123 instance
            var instance = new InstanceMethodTest("aaa");
            var expression =
                Apply(
                    Apply(
                        Variable("VoidOverload"),
                        Constant(123)),
                    Constant(instance));

            var actual = environments.Reduce(expression);

            // ()
            var expected =
                Unit();

            AssertLogicalEqual(expression, expected, actual);
            Assert.AreEqual("aaaabc123", instance.fv.ToString());
        }
   
        [Test]
        public void ApplyInstanceMethod6()
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers<InstanceMethodTest>();
            
            // VoidOverload 123 123.456 instance
            var instance = new InstanceMethodTest("aaa");
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Variable("VoidOverload"),
                            Constant(123)),
                        Constant(123.456)),
                    Constant(instance));

            var actual = environments.Reduce(expression);

            // ()
            var expected =
                Unit();

            AssertLogicalEqual(expression, expected, actual);
            Assert.AreEqual("aaaabc123_123.456", instance.fv.ToString());
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
        #endregion

        #region Method overloads
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
            var typeTerm = environments.MutableBindTypeAndMembers(typeof(OverloadTest1));
            
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
        [TestCase("a1", "b2", "a1b2")]
        public void ApplyOverloadedMethod2(object a, object b, object r)
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers(typeof(OverloadTest2));
            
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
        
        public static class OverloadTest3
        {
            public static int Overload(int a, int b, int c) =>
                a + b + c;
            public static double Overload(double a, double b, double c) =>
                a + b + c;
            public static string Overload(string a, string b, string c) =>
                a + b + c;
        }

        [TestCase(1, 2, 3, 6)]
        [TestCase(1.0, 2.0, 3.0, 6.0)]
        [TestCase("a1", "b2", "c3", "a1b2c3")]
        public void ApplyOverloadedMethod3(object a, object b, object c, object r)
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers(typeof(OverloadTest3));
            
            // OverloadTest3.Overload(1, 2, 3)
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Variable("Favalet.Reducing.OverloadTest3.Overload"),
                            Constant(a)),
                        Constant(b)),
                    Constant(c));

            var actual = environments.Reduce(expression);

            // 6
            var expected =
                Constant(r);

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion

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
            var typeTerm = environments.MutableBindTypeAndMembers(typeof(TypeConstructorTest<>));

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

        #region From delegate
        [Test]
        public void StaticMethodDelegate1()
        {
            var environments = CLREnvironments();

            // int.Parse "123"
            var expression =
                Apply(
                    Delegate<string, int>(int.Parse),
                    Constant("123"));

            var actual = environments.Reduce(expression);

            // 123
            var expected =
                Constant(123);

            AssertLogicalEqual(expression, expected, actual);
        }

        public static void VoidMethod(StringBuilder sb, int value) =>
            sb.Append(value);
        
        [Test]
        public void StaticMethodDelegate2()
        {
            var environments = CLREnvironments();

            // VoidMethod sb "123"
            var sb = new StringBuilder();
            var expression =
                Apply(
                    Apply(
                        Delegate<StringBuilder, int>(VoidMethod),
                        Constant(sb)),
                    Constant(123));

            var actual = environments.Reduce(expression);

            // ()
            var expected =
                Unit();

            AssertLogicalEqual(expression, expected, actual);
            Assert.AreEqual("123", sb.ToString());
        }
        
        [Test]
        public void InstanceMethodDelegate1()
        {
            var environments = CLREnvironments();

            // DateTime.Now.Add timeSpan
            var now = DateTime.Now;
            var timeSpan = TimeSpan.FromSeconds(100);
            var expression =
                Apply(
                    Delegate<TimeSpan, DateTime>(now.Add),
                    Constant(timeSpan));

            var actual = environments.Reduce(expression);

            // [DateTime+timeSpan]
            var expected =
                Constant(now + timeSpan);

            AssertLogicalEqual(expression, expected, actual);
        }
        
        public sealed class InstanceMethodDelegateTest
        {
            public readonly StringBuilder sb = new StringBuilder();
            
            public InstanceMethodDelegateTest()
            {
            }

            public void VoidMethod(int value) =>
                sb.Append(value);
        }
        
        [Test]
        public void InstanceMethodDelegate2()
        {
            var environments = CLREnvironments();

            // instance.VoidMethod 123
            var instance = new InstanceMethodDelegateTest();
            var expression =
                Apply(
                    Delegate<int>(instance.VoidMethod),
                    Constant(123));

            var actual = environments.Reduce(expression);

            // ()
            var expected =
                Unit();

            AssertLogicalEqual(expression, expected, actual);
            Assert.AreEqual("123", instance.sb.ToString());
        }

        [Test]
        public void ExtensionMethodDelegate1()
        {
            var environments = CLREnvironments();

            // uri.UriMethod 123
            var uri = new Uri("https://example.com/", UriKind.RelativeOrAbsolute);
            var expression =
                Apply(
                    Delegate<int, string>(uri.UriMethod),
                    Constant(123));

            var actual = environments.Reduce(expression);

            // "https://example.com/123"
            var expected =
                Constant("https://example.com/123");

            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void ExtensionMethodDelegate2()
        {
            var environments = CLREnvironments();

            // sb.VoidMethod 1 2
            var sb = new StringBuilder();
            var expression =
                Apply(
                    Apply(
                        Delegate<int, int>(sb.VoidMethod),
                        Constant(1)),
                    Constant(2));

            var actual = environments.Reduce(expression);

            // ()
            var expected =
                Unit();

            AssertLogicalEqual(expression, expected, actual);
            Assert.AreEqual("12", sb.ToString());
        }
        #endregion
    }

    internal static class ExtensionMethodTest
    {
        public static int Method(this int a, int b, int c) =>
            a + b * 10 + c * 100;
        public static void VoidMethod(this StringBuilder a, int b, int c) =>
            a.Append(b * 10 + c);
        public static string UriMethod(this Uri url, int number) =>
            url.ToString() + number;
    }
}
