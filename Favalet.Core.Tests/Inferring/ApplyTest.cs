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

using static Favalet.CLRGenerator;
using static Favalet.Generator;

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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        // TODO: Unmatched orders
    }
}
