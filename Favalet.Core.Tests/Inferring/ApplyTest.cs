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
using static Favalet.CLRGenerator;
using static Favalet.Generator;
using static Favalet.TestUtiltiies;

namespace Favalet.Inferring
{
    [TestFixture]
    public sealed class ApplyTest
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
        
        private static ApplyExpression Apply(
            IExpression function, IExpression argument) =>
            ApplyExpression.Create(function, argument, Ranges.TextRange.Unknown);
        private static ApplyExpression Apply(
            IExpression function, IExpression argument, IExpression higherOrder) =>
            ApplyExpression.UnsafeCreate(function, argument, higherOrder, Ranges.TextRange.Unknown);

        #region Without annotation
        [Test]
        public void ApplyWithoutAnnotation()
        {
            var environments = Environments();

            // a b
            var expression =
                Apply(
                    Variable("a"),
                    Variable("b"));

            var actual = environments.Infer(expression);

            // (a:('0 -> '1) b:'0):'1
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var ph1 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Variable("a", Lambda(ph0, ph1)),
                    Variable("b", ph0),
                    ph1);

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyNestedFunctionWithoutAnnotation1()
        {
            var environments = Environments();

            // a a
            var expression =
                Apply(
                    Variable("a"),
                    Variable("a"));

            var actual = environments.Infer(expression);

            // (a:('0 -> '1) a:'0):'1
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var ph1 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Variable("a", Lambda(ph0, ph1)),
                    Variable("a", ph0),
                    ph1);

            AssertLogicalEqual(expression, expected, actual);
        }

        //[Test]
        public void ApplyNestedFunctionWithoutAnnotation2()
        {
            var environments = Environments();

            // a = x -> x
            environments.MutableBind(
                "a",
                Lambda(
                    "x",
                    Variable("x")));

            // a a
            var expression =
                Apply(
                    Variable("a"),
                    Variable("a"));

            var actual = environments.Infer(expression);

            // ((x:('0 -> '0) -> x:('0 -> '0)) (x:'0 -> x:'0)):('0 -> '0)
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Variable(
                        "a",
                        Lambda(
                            Lambda(ph0, ph0),
                            Lambda(ph0, ph0))),
                    Variable(
                        "a",
                        Lambda(ph0, ph0)),
                    Lambda(
                        ph0,
                        ph0));

            AssertLogicalEqual(expression, expected, actual);
        }
 
        [Test]
        public void NestedApplyWithoutAnnotation()
        {
            var environments = Environments();

            // a b c
            var expression =
                Apply(
                    Apply(
                        Variable("a"),
                        Variable("b")),
                    Variable("c"));

            var actual = environments.Infer(expression);

            // ((a:('0 -> ('1 -> '2)) b:'0 c:'1):'2
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var ph1 = provider.CreatePlaceholder();
            var ph2 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Apply(
                        Variable("a",
                            Lambda(
                                ph0,
                                Lambda(ph1, ph2))),
                        Variable("b", ph0)),
                    Variable("c", ph1),
                    ph2);

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion

        #region With annotation
        [Test]
        public void ApplyWithAnnotation1()
        {
            var environments = Environments();

            // a:(bool -> _) b
            var expression =
                Apply(
                    Variable("a",
                        Lambda(
                            Variable("bool"),
                            Unspecified())),
                    Variable("b"));

            var actual = environments.Infer(expression);

            // (a:(bool -> '0) b:bool):'0
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Variable("a",
                        Lambda(Variable("bool"), ph0)),
                    Variable("b", Variable("bool")),
                    ph0);

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyWithAnnotation2()
        {
            var environments = Environments();

            // a:(_ -> bool) b
            var expression =
                Apply(
                    Variable("a",
                        Lambda(
                            Unspecified(),
                            Variable("bool"))),
                    Variable("b"));

            var actual = environments.Infer(expression);

            // (a:('0 -> bool) b:'0):bool
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Variable("a",
                        Lambda(ph0, Variable("bool"))),
                    Variable("b", ph0),
                    Variable("bool"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyWithAnnotation3()
        {
            var environments = Environments();

            // a:(int -> bool) b
            var expression =
                Apply(
                    Variable("a",
                        Lambda(
                            Variable("int"),
                            Variable("bool"))),
                    Variable("b"));

            var actual = environments.Infer(expression);

            // (a:(int -> bool) b:int):bool
            var expected =
                Apply(
                    Variable("a",
                        Lambda(
                            Variable("int"),
                            Variable("bool"))),
                    Variable("b", Variable("int")),
                    Variable("bool"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyWithAnnotation4()
        {
            var environments = Environments();

            // a b:bool
            var expression =
                Apply(
                    Variable("a"),
                    Variable("b", Variable("bool")));

            var actual = environments.Infer(expression);

            // (a:(bool -> _) b:bool):_
            var expected =
                Apply(
                    Variable("a",
                        Lambda(
                            Variable("bool"),
                            Unspecified())),
                    Variable("b", Variable("bool")),
                    Unspecified());

            AssertLogicalEqual(expression, expected, actual);
        }

        //[Test]
        public void ApplyWithAnnotation5()
        {
            var environments = Environments();

            // (a b):bool
            var expression =
                Apply(
                    Variable("a"),
                    Variable("b"),
                    Variable("bool"));

            var actual = environments.Infer(expression);

            // (a:('0 -> bool) b:'0):bool
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Variable("a",
                        Lambda(
                            ph0,
                            Variable("bool"))),
                    Variable("b", ph0),
                    Variable("bool"));

            AssertLogicalEqual(expression, expected, actual);
        }

        //[Test]
        public void ApplyWithAnnotation6()
        {
            var environments = Environments();

            // (a b:bool):int
            var expression =
                Apply(
                    Variable("a"),
                    Variable("b", Variable("bool")),
                    Variable("int"));

            var actual = environments.Infer(expression);

            // (a:(bool -> int) b:bool):int
            var expected =
                Apply(
                    Variable("a",
                        Lambda(
                            Variable("bool"),
                            Variable("int"))),
                    Variable("b", Variable("bool")),
                    Variable("int"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyWithAnnotation7()
        {
            var environments = Environments();

            // a:(_ -> int) b:bool
            var expression =
                Apply(
                    Variable("a",
                        Lambda(
                            Unspecified(),
                            Variable("int"))),
                    Variable("b", Variable("bool")));

            var actual = environments.Infer(expression);

            // (a:(bool -> int) b:bool):int
            var expected =
                Apply(
                    Variable("a",
                        Lambda(
                            Variable("bool"),
                            Unspecified())),
                    Variable("b", Variable("bool")),
                    Unspecified());

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyWithAnnotation8()
        {
            var environments = Environments();

            // a:(_ -> int) b:bool
            var expression =
                Apply(
                    Variable("a",
                        Lambda(
                            Unspecified(),
                            Variable("int"))),
                    Variable("b", Variable("bool")));

            var actual = environments.Infer(expression);

            // (a:(bool -> int) b:bool):int
            var expected =
                Apply(
                    Variable("a",
                        Lambda(
                            Variable("bool"),
                            Unspecified())),
                    Variable("b", Variable("bool")),
                    Unspecified());

            AssertLogicalEqual(expression, expected, actual);
        }

        //[Test]
        public void ApplyWithAnnotation9()
        {
            var environments = Environments();

            // (a:(bool -> _) b):int
            var expression =
                Apply(
                    Variable("a",
                        Lambda(
                            Variable("bool"),
                            Unspecified())),
                    Variable("b"),
                    Variable("int"));

            var actual = environments.Infer(expression);

            // (a:(bool -> int) b:bool):int
            var expected =
                Apply(
                    Variable("a",
                        Lambda(
                            Variable("bool"),
                            Variable("int"))),
                    Variable("b", Variable("bool")),
                    Variable("int"));

            AssertLogicalEqual(expression, expected, actual);
        }

        //[Test]
        public void ApplyWithAnnotation10()
        {
            var environments = Environments();

            // (a:(bool -> int) b:bool):int
            var expression =
                Apply(
                    Variable("a",
                        Lambda(
                            Variable("bool"),
                            Variable("int"))),
                    Variable("b", Variable("bool")),
                    Variable("int"));

            var actual = environments.Infer(expression);

            // (a:(bool -> int) b:bool):int
            var expected =
                Apply(
                    Variable("a",
                        Lambda(
                            Variable("bool"),
                            Variable("int"))),
                    Variable("b", Variable("bool")),
                    Variable("int"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void NestedApplyWithAnnotation1()
        {
            var environments = Environments();

            // a b c:bool
            var expression =
                Apply(
                    Apply(
                        Variable("a"),
                        Variable("b")),
                    Variable("c", Variable("bool")));

            var actual = environments.Infer(expression);

            // ((a:('0 -> (bool -> '1)) b:'0 c:bool):'1
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var ph1 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Apply(
                        Variable("a",
                            Lambda(
                                ph0,
                                Lambda(Variable("bool"), ph1))),
                        Variable("b", ph0)),
                    Variable("c", Variable("bool")),
                    ph1);

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void NestedApplyWithAnnotation2()
        {
            var environments = Environments();

            // a b:bool c
            var expression =
                Apply(
                    Apply(
                        Variable("a"),
                        Variable("b", Variable("bool"))),
                    Variable("c"));

            var actual = environments.Infer(expression);

            // ((a:(bool -> ('0 -> '1)) b:bool c:'0):'1
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var ph1 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Apply(
                        Variable("a",
                            Lambda(
                                Variable("bool"),
                                Lambda(ph0, ph1))),
                        Variable("b", Variable("bool"))),
                    Variable("c", ph0),
                    ph1);

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void NestedApplyWithAnnotation3()
        {
            var environments = Environments();

            // a:(bool -> _) b c
            var expression =
                Apply(
                    Apply(
                        Variable("a",
                            Lambda(Variable("bool"), Unspecified())),
                        Variable("b")),
                    Variable("c"));

            var actual = environments.Infer(expression);

            // ((a:(bool -> ('0 -> '1)) b:bool c:'0):'1
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var ph1 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Apply(
                        Variable("a",
                            Lambda(
                                Variable("bool"),
                                Lambda(ph0, ph1))),
                        Variable("b", Variable("bool"))),
                    Variable("c", ph0),
                    ph1);

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion

        #region Y combinators
        //[Test]
        public void ApplyYCombinator1()
        {
            var environments = Environments();

            // Y = f -> f (Y f)
            environments.MutableBind(
                "Y",
                Lambda(
                    "f",
                    Apply(
                        Variable("f"),
                        Apply(
                            Variable("Y"),
                            Variable("f")))));

            // Y
            var expression =
                Variable("Y");

            var actual = environments.Infer(expression);

            var expected =
                Variable("TODO:");

            AssertLogicalEqual(expression, expected, actual);
        }

        //[Test]
        public void ApplyYCombinator2()
        {
            var environments = Environments();

            // Y = f -> (x -> f (x x)) (x -> f (x x))
            var expression =
                Lambda(
                    "f",
                    Apply(
                        Lambda( // x -> f (x x)
                            "x",
                            Apply(
                                Variable("f"),
                                Apply(
                                    Variable("x"),
                                    Variable("x")))),
                        Lambda( // x -> f (x x)
                            "x",
                            Apply(
                                Variable("f"),
                                Apply(
                                    Variable("x"),
                                    Variable("x"))))));

            var actual = environments.Infer(expression);

            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Variable("TODO:");

            AssertLogicalEqual(expression, expected, actual);
        }

        //[Test]
        public void ApplyZCombinator()
        {
            var environments = Environments();

            // Z = f -> (x -> f (y -> x x y)) (x -> f (y -> x x y))
            var expression =
                Lambda(
                    "f",
                    Apply(
                        Lambda( // x -> f (y -> x x y)
                            "x",
                            Apply(
                                Variable("f"),
                                Lambda( // y -> x x y
                                    "y",
                                    Apply(
                                        Apply(
                                            Variable("x"),
                                            Variable("x")),
                                        Variable("y"))))),
                        Lambda( // x -> f (y -> x x y)
                            "x",
                            Apply(
                                Variable("f"),
                                Lambda( // y -> x x y
                                    "y",
                                    Apply(
                                        Apply(
                                            Variable("x"),
                                            Variable("x")),
                                        Variable("y")))))));

            var actual = environments.Infer(expression);

            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Variable("TODO:");

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion

        #region Methods
        public sealed class StaticMethodTest
        {
            public static double Method(string value) =>
                throw new InvalidOperationException();
            public static void VoidMethod(StringBuilder sb, int value) =>
                throw new InvalidOperationException();
        }

        [Test]
        public void ApplyStaticMethod1()
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers<StaticMethodTest>();

            // StaticMethodTest.Method "123.456"
            var expression =
                Apply(
                    Variable("Favalet.Inferring.StaticMethodTest.Method"),
                    Constant("123.456"));

            var actual = environments.Infer(expression);

            // ((StaticMethodTest.Method:(string -> double) "123.456"):double
            var expected =
                Apply(
                    Variable(
                        "Favalet.Inferring.StaticMethodTest.Method",
                        Lambda(Type<string>(), Type<double>())),
                    Constant("123.456"),
                    Type<double>());

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
                        Variable("Favalet.Inferring.StaticMethodTest.VoidMethod"),
                        Constant(sb)),
                    Constant(123));

            var actual = environments.Infer(expression);

            // ((StaticMethodTest.VoidMethod:(StringBuilder -> int -> unit) sb:StringBuilder 123:int):unit
            var expected =
                Apply(
                    Apply(
                        Variable(
                            "Favalet.Inferring.StaticMethodTest.VoidMethod",
                            Lambda(Type<StringBuilder>(), Lambda(Type<int>(), UnitType()))),
                        Constant(sb),
                        Lambda(Type<int>(), UnitType())),
                    Constant(123),
                    UnitType());

            AssertLogicalEqual(expression, expected, actual);
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
                            Variable("Favalet.Inferring.ExtensionMethodTest.Method"),
                            Constant(2)),
                        Constant(3)),
                    Constant(1));

            var actual = environments.Infer(expression);

            // (((Method:(int -> int -> int -> int) 2:int):(int -> int -> int) 3:int):(int -> int) 1:int):int
            var expected =
                Apply(
                    Apply(
                        Apply(
                            Variable(
                                "Favalet.Inferring.ExtensionMethodTest.Method",
                                Lambda(Type<int>(), Lambda(Type<int>(), Lambda(Type<int>(), Type<int>())))),
                            Constant(2),
                            Lambda(Type<int>(), Lambda(Type<int>(), Type<int>()))),
                        Constant(3)),
                    Constant(1),
                    Type<int>());

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
                            Variable("Favalet.Inferring.ExtensionMethodTest.VoidMethod"),
                            Constant(2)),
                        Constant(3)),
                    Constant(sb));

            var actual = environments.Infer(expression);

            // (((VoidMethod:(int -> int -> StringBuilder -> unit) 2:int):(int -> StringBuilder -> unit) 3:int):(StringBuilder -> unit) sb:StringBuilder):unit
            var expected =
                Apply(
                    Apply(
                        Apply(
                            Variable(
                                "Favalet.Inferring.ExtensionMethodTest.VoidMethod",
                                Lambda(Type<int>(), Lambda(Type<int>(), Lambda(Type<StringBuilder>(), UnitType())))),
                            Constant(2),
                            Lambda(Type<int>(), Lambda(Type<StringBuilder>(), UnitType()))),
                        Constant(3)),
                    Constant(sb),
                    UnitType());

            AssertLogicalEqual(expression, expected, actual);
        }

        public sealed class InstanceMethodTest
        {
            public InstanceMethodTest()
            { }

            public string Overload() =>
                throw new InvalidOperationException();
            public string Overload(int value) =>
                throw new InvalidOperationException();
            public string Overload(int value1, double value2) =>
                throw new InvalidOperationException();
            public void VoidOverload() =>
                throw new InvalidOperationException();
            public void VoidOverload(int value) =>
                throw new InvalidOperationException();
            public void VoidOverload(int value1, double value2) =>
                throw new InvalidOperationException();
        }

        [Test]
        public void ApplyInstanceMethod1()
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers<InstanceMethodTest>();
            
            // Overload instance
            var instance = new InstanceMethodTest();
            var expression =
                Apply(
                    Variable("Overload"),
                    Constant(instance));

            var actual = environments.Infer(expression);

            // (Overload:(InstanceMethodTest -> string) instance:InstanceMethodTest)):string
            var expected =
                Apply(
                    Variable(
                        "Overload",
                        Lambda(Type<InstanceMethodTest>(), Type<string>())),
                    Constant(instance),
                    Type<string>());

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyInstanceMethod2()
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers<InstanceMethodTest>();
            
            // Overload 123 instance
            var instance = new InstanceMethodTest();
            var expression =
                Apply(
                    Apply(
                        Variable("Overload"),
                        Constant(123)),
                    Constant(instance));

            var actual = environments.Infer(expression);

            // ((Overload:(int -> InstanceMethodTest -> string) 123:int):(InstanceMethodTest -> string) instance:InstanceMethodTest)):string
            var expected =
                Apply(
                    Apply(
                        Variable(
                            "Overload",
                            Lambda(Type<int>(), Lambda(Type<InstanceMethodTest>(), Type<string>()))),
                        Constant(123),
                        Lambda(Type<InstanceMethodTest>(), Type<string>())),
                    Constant(instance),
                    Type<string>());

            AssertLogicalEqual(expression, expected, actual);
        }
   
        [Test]
        public void ApplyInstanceMethod3()
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers<InstanceMethodTest>();
            
            // Overload 123 123.456 instance
            var instance = new InstanceMethodTest();
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Variable("Overload"),
                            Constant(123)),
                        Constant(123.456)),
                    Constant(instance));

            var actual = environments.Infer(expression);

            // (((Overload:(int -> double -> InstanceMethodTest -> string) 123:int):(double -> InstanceMethodTest -> string) 123.456:double) instance:InstanceMethodTest)):string
            var expected =
                Apply(
                    Apply(
                        Apply(
                            Variable(
                                "Overload",
                                Lambda(Type<int>(), Lambda(Type<double>(), Lambda(Type<InstanceMethodTest>(), Type<string>())))),
                            Constant(123),
                            Lambda(Type<double>(), Lambda(Type<InstanceMethodTest>(), Type<string>()))),
                        Constant(123.456),
                        Lambda(Type<InstanceMethodTest>(), Type<string>())),
                    Constant(instance),
                    Type<string>());

            AssertLogicalEqual(expression, expected, actual);
        }
     
        [Test]
        public void ApplyInstanceMethod4()
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers<InstanceMethodTest>();
            
            // VoidOverload instance
            var instance = new InstanceMethodTest();
            var expression =
                Apply(
                    Variable("VoidOverload"),
                    Constant(instance));

            var actual = environments.Infer(expression);

            // (VoidOverload:(InstanceMethodTest -> unit) instance:InstanceMethodTest)):unit
            var expected =
                Apply(
                    Variable(
                        "VoidOverload",
                        Lambda(Type<InstanceMethodTest>(), UnitType())),
                    Constant(instance),
                    UnitType());

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyInstanceMethod5()
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers<InstanceMethodTest>();
            
            // VoidOverload 123 instance
            var instance = new InstanceMethodTest();
            var expression =
                Apply(
                    Apply(
                        Variable("VoidOverload"),
                        Constant(123)),
                    Constant(instance));

            var actual = environments.Infer(expression);

            // ((VoidOverload:(int -> InstanceMethodTest -> unit) 123:int):(InstanceMethodTest -> unit) instance:InstanceMethodTest)):unit
            var expected =
                Apply(
                    Apply(
                        Variable(
                            "VoidOverload",
                            Lambda(Type<int>(), Lambda(Type<InstanceMethodTest>(), UnitType()))),
                        Constant(123),
                        Lambda(Type<InstanceMethodTest>(), UnitType())),
                    Constant(instance),
                    UnitType());

            AssertLogicalEqual(expression, expected, actual);
        }
   
        [Test]
        public void ApplyInstanceMethod6()
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers<InstanceMethodTest>();
            
            // VoidOverload 123 123.456 instance
            var instance = new InstanceMethodTest();
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Variable("VoidOverload"),
                            Constant(123)),
                        Constant(123.456)),
                    Constant(instance));

            var actual = environments.Infer(expression);

            // (((VoidOverload:(int -> double -> InstanceMethodTest -> unit) 123:int):(double -> InstanceMethodTest -> unit) 123.456:double) instance:InstanceMethodTest)):unit
            var expected =
                Apply(
                    Apply(
                        Apply(
                            Variable(
                                "VoidOverload",
                                Lambda(Type<int>(), Lambda(Type<double>(), Lambda(Type<InstanceMethodTest>(), UnitType())))),
                            Constant(123),
                            Lambda(Type<double>(), Lambda(Type<InstanceMethodTest>(), UnitType()))),
                        Constant(123.456),
                        Lambda(Type<InstanceMethodTest>(), UnitType())),
                    Constant(instance),
                    UnitType());

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

            var actual = environment.Infer(expression);

            // (Uri:(string -> Uri) "http://example.com":string):Uri
            var expected =
                Apply(
                    Method(typeof(Uri).GetConstructor(new[] { typeof(string) })!),
                    Constant("http://example.com"),
                    Type<Uri>());

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion

        #region Method overloads
        public static class OverloadTest1
        {
            public static string Overload(int value) =>
                throw new InvalidOperationException();
            public static string Overload(double value) =>
                throw new InvalidOperationException();
            public static string Overload(string value) =>
                throw new InvalidOperationException();
        }

        [TestCase(123, "123")]
        [TestCase(123.456, "123.456")]
        [TestCase("abc", "abc")]
        public void ApplyOverloadedMethod1(object argument, object result)
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers(typeof(OverloadTest1));
            
            // OverloadTest1.Overload 123
            var expression =
                Apply(
                    Variable("Favalet.Inferring.OverloadTest1.Overload"),
                    Constant(argument));

            var actual = environments.Infer(expression);

            // (OverloadTest1.Overload:(int -> string) 123:int):string
            var expected =
                Apply(
                    Variable(
                        "Favalet.Inferring.OverloadTest1.Overload",
                        Lambda(Type(argument.GetType()), Type(result.GetType()))),
                    Constant(argument),
                    Type<string>());

            AssertLogicalEqual(expression, expected, actual);
        }
        
        public static class OverloadTest2
        {
            public static int Overload(int a, int b) =>
                throw new InvalidOperationException();
            public static double Overload(double a, double b) =>
                throw new InvalidOperationException();
            public static string Overload(string a, string b) =>
                throw new InvalidOperationException();
        }

        [TestCase(1, 2, 3)]
        [TestCase(1.0, 2.0, 3.0)]
        [TestCase("a1", "b2", "a1b2")]
        public void ApplyOverloadedMethod2(object a, object b, object r)
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers(typeof(OverloadTest2));
            
            // OverloadTest2.Overload 1 2
            var expression =
                Apply(
                    Apply(
                        Variable("Favalet.Inferring.OverloadTest2.Overload"),
                        Constant(a)),
                    Constant(b));

            var actual = environments.Infer(expression);

            // ((OverloadTest2.Overload:(int -> int -> string) 1:int):(int -> string) 2:int):string
            var expected =
                Apply(
                    Apply(
                        Variable(
                            "Favalet.Inferring.OverloadTest2.Overload",
                            Lambda(Type(a.GetType()), Lambda(Type(b.GetType()), Type(r.GetType())))),
                        Constant(a),
                        Lambda(Type(b.GetType()), Type(r.GetType()))),
                    Constant(b),
                    Type(r.GetType()));

            AssertLogicalEqual(expression, expected, actual);
        }
        
        public static class OverloadTest3
        {
            public static int Overload(int a, int b, int c) =>
                throw new InvalidOperationException();
            public static double Overload(double a, double b, double c) =>
                throw new InvalidOperationException();
            public static string Overload(string a, string b, string c) =>
                throw new InvalidOperationException();
        }

        [TestCase(1, 2, 3, 6)]
        [TestCase(1.0, 2.0, 3.0, 6.0)]
        [TestCase("a1", "b2", "c3", "a1b2c3")]
        public void ApplyOverloadedMethod3(object a, object b, object c, object r)
        {
            var environments = CLREnvironments();
            var typeTerm = environments.MutableBindTypeAndMembers(typeof(OverloadTest3));
            
            // OverloadTest3.Overload 1 2 3
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Variable("Favalet.Inferring.OverloadTest3.Overload"),
                            Constant(a)),
                        Constant(b)),
                    Constant(c));

            var actual = environments.Infer(expression);

            // (((OverloadTest2.Overload:(int -> int -> int -> string) 1:int):(int -> int -> string) 2:int):(int -> string) 3:int):string
            var expected =
                Apply(
                    Apply(
                        Apply(
                            Variable(
                                "Favalet.Inferring.OverloadTest3.Overload",
                                Lambda(Type(a.GetType()), Lambda(Type(b.GetType()), Lambda(Type(c.GetType()), Type(r.GetType()))))),
                            Constant(a),
                            Lambda(Type(b.GetType()), Lambda(Type(c.GetType()), Type(r.GetType())))),
                        Constant(b),
                        Lambda(Type(c.GetType()), Type(r.GetType()))),
                    Constant(c),
                    Type(r.GetType()));

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion
    }
        
    public static class ExtensionMethodTest
    {
        public static int Method(this int a, int b, int c) =>
            throw new InvalidOperationException();
        public static void VoidMethod(this StringBuilder a, int b, int c) =>
            throw new InvalidOperationException();
    }
}
