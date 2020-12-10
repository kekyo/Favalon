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
    public sealed class LambdaTest
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
        public void LambdaWithoutAnnotation1()
        {
            var environments = Environments();

            // a -> a
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a"));

            var actual = environments.Infer(expression);

            // (a:'0 -> a:'0):('0 -> '0)
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Lambda(
                    BoundVariable("a", ph0),
                    Variable("a", ph0),
                    Lambda(ph0, ph0));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithoutAnnotation2()
        {
            var environments = Environments();

            // a -> b
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("b"));

            var actual = environments.Infer(expression);

            // (a:'0 -> b:'1):('0 -> '1)
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var ph1 = provider.CreatePlaceholder();
            var expected =
                Lambda(
                    BoundVariable("a", ph0),
                    Variable("b", ph1),
                    Lambda(ph0, ph1));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation1()
        {
            var environments = Environments();

            // a:bool -> a
            var expression =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("a"));

            var actual = environments.Infer(expression);

            // (a:bool -> a:bool):(bool -> bool)
            var expected =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("a", Type<bool>()),
                    Lambda(Type<bool>(), Type<bool>()));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation2()
        {
            var environments = Environments();

            // a -> a:bool
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a", Type<bool>()));

            var actual = environments.Infer(expression);

            // (a:bool -> a:bool):(bool -> bool)
            var expected =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("a", Type<bool>()),
                    Lambda(Type<bool>(), Type<bool>()));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation3()
        {
            var environments = Environments();

            // a:bool -> a:bool
            var expression =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("a", Type<bool>()));

            var actual = environments.Infer(expression);

            // (a:bool -> a:bool):(bool -> bool)
            var expected =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("a", Type<bool>()),
                    Lambda(Type<bool>(), Type<bool>()));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation4()
        {
            var environments = Environments();

            // (a -> a):(bool -> _)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a"),
                    Lambda(
                        Type<bool>(),
                        Unspecified()));

            var actual = environments.Infer(expression);

            // (a:bool -> a:bool):(bool -> bool)
            var expected =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("a", Type<bool>()),
                    Lambda(Type<bool>(), Type<bool>()));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation5()
        {
            var environments = Environments();

            // (a -> a):(_ -> bool)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a"),
                    Lambda(
                        Unspecified(),
                        Type<bool>()));

            var actual = environments.Infer(expression);

            // (a:bool -> a:bool):(bool -> bool)
            var expected =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("a", Type<bool>()),
                    Lambda(Type<bool>(), Type<bool>()));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation6()
        {
            var environments = Environments();

            // (a -> a):(bool -> bool)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a"),
                    Lambda(
                        Type<bool>(),
                        Type<bool>()));

            var actual = environments.Infer(expression);

            // (a:bool -> a:bool):(bool -> bool)
            var expected =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("a", Type<bool>()),
                    Lambda(Type<bool>(), Type<bool>()));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation7()
        {
            var environments = Environments();

            // (a -> a):(_ -> _)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a"),
                    Lambda(
                        Unspecified(),
                        Unspecified()));

            var actual = environments.Infer(expression);

            // (a:'0 -> a:'0):('0 -> '0)
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Lambda(
                    BoundVariable("a", ph0),
                    Variable("a", ph0),
                    Lambda(ph0, ph0));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation8()
        {
            var environments = Environments();

            // (a -> b):(bool -> _)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("b"),
                    Lambda(
                        Type<bool>(),
                        Unspecified()));

            var actual = environments.Infer(expression);

            // (a:bool -> b:'0):(bool -> '0)
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("b", ph0),
                    Lambda(Type<bool>(), ph0));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation9()
        {
            var environments = Environments();

            // (a -> b):(_ -> bool)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("b"),
                    Lambda(
                        Unspecified(),
                        Type<bool>()));

            var actual = environments.Infer(expression);

            // (a:'0 -> b:bool):('0 -> bool)
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Lambda(
                    BoundVariable("a", ph0),
                    Variable("b", Type<bool>()),
                    Lambda(ph0, Type<bool>()));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation10()
        {
            var environments = Environments();

            // (a -> b):(bool -> bool)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("b"),
                    Lambda(
                        Type<bool>(),
                        Type<bool>()));

            var actual = environments.Infer(expression);

            // (a:bool -> b:bool):(bool -> bool)
            var expected =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("b", Type<bool>()),
                    Lambda(Type<bool>(), Type<bool>()));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation11()
        {
            var environments = Environments();

            // (a -> b:bool):(bool -> _)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("b", Type<bool>()),
                    Lambda(
                        Type<bool>(),
                        Unspecified()));

            var actual = environments.Infer(expression);

            // (a:bool -> b:bool):(bool -> bool)
            var expected =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("b", Type<bool>()),
                    Lambda(Type<bool>(), Type<bool>()));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation12()
        {
            var environments = Environments();

            // (a:bool -> b):(_ -> bool)
            var expression =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("b"),
                    Lambda(
                        Unspecified(),
                        Type<bool>()));

            var actual = environments.Infer(expression);

            // (a:bool -> b:bool):(bool -> bool)
            var expected =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("b", Type<bool>()),
                    Lambda(Type<bool>(), Type<bool>()));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation13_1()
        {
            var environments = Environments();

            // (a -> a):(bool -> _)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a"),
                    Lambda(
                        Variable("bool"),
                        Unspecified()));

            var actual = environments.Infer(expression);

            // (a:bool -> a:bool):(bool -> bool)
            var expected =
                Lambda(
                    BoundVariable("a", Variable("bool")),
                    Variable("a", Variable("bool")),
                    Lambda(
                        Variable("bool"),
                        Variable("bool")));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation13_2()
        {
            var environments = Environments();

            // (a -> a):(bool -> _):(* -> *)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a"),
                    Lambda(
                        Variable("bool"),
                        Unspecified(),
                        Lambda(
                            Kind(),
                            Kind())));

            var actual = environments.Infer(expression);

            // (a:bool -> a:bool):(bool -> bool):(* -> *)
            var expected =
                Lambda(
                    BoundVariable("a", Variable("bool", Kind())),
                    Variable("a", Variable("bool", Kind())),
                    Lambda(
                        Variable("bool", Kind()),
                        Variable("bool", Kind()),
                        Lambda(
                            Kind(),
                            Kind())));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation14_1()
        {
            var environments = Environments();

            // (a -> a):(_ -> bool)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a"),
                    Lambda(
                        Unspecified(),
                        Variable("bool")));

            var actual = environments.Infer(expression);

            // (a:bool -> a:bool):(bool -> bool)
            var expected =
                Lambda(
                    BoundVariable("a", Variable("bool")),
                    Variable("a", Variable("bool")),
                    Lambda(
                        Variable("bool"),
                        Variable("bool")));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation14_2()
        {
            var environments = Environments();

            // (a -> a):(_ -> bool):(* -> *)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a"),
                    Lambda(
                        Unspecified(),
                        Variable("bool"),
                        Lambda(
                            Kind(),
                            Kind())));

            var actual = environments.Infer(expression);

            // (a:bool -> a:bool):(bool -> bool):(* -> *)
            var expected =
                Lambda(
                    BoundVariable("a", Variable("bool", Kind())),
                    Variable("a", Variable("bool", Kind())),
                    Lambda(
                        Variable("bool", Kind()),
                        Variable("bool", Kind()),
                        Lambda(
                            Kind(),
                            Kind())));

            var r = expected.GetPrettyString(PrettyStringTypes.Readable);

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation15_1()
        {
            var environments = Environments();

            // (a -> a):(bool -> bool)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a"),
                    Lambda(
                        Variable("bool"),
                        Variable("bool")));

            var actual = environments.Infer(expression);

            // (a:bool -> a:bool):(bool -> bool)
            var expected =
                Lambda(
                    BoundVariable("a", Variable("bool")),
                    Variable("a", Variable("bool")),
                    Lambda(
                        Variable("bool"),
                        Variable("bool")));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation15_2()
        {
            var environments = Environments();

            // (a -> a):(bool -> bool):(* -> *)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a"),
                    Lambda(
                        Variable("bool"),
                        Variable("bool"),
                        Lambda(
                            Kind(),
                            Kind())));

            var actual = environments.Infer(expression);

            // (a:bool -> a:bool):(bool -> bool):(* -> *)
            var expected =
                Lambda(
                    BoundVariable("a", Variable("bool", Kind())),
                    Variable("a", Variable("bool", Kind())),
                    Lambda(
                        Variable("bool", Kind()),
                        Variable("bool", Kind()),
                        Lambda(
                            Kind(),
                            Kind())));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation16()
        {
            var environments = Environments();

            // (a -> a):(bool -> _):(* -> _)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a"),
                    Lambda(
                        Variable("bool"),
                        Unspecified(),
                        Lambda(
                            Kind(),
                            Unspecified())));

            var actual = environments.Infer(expression);

            // (a:bool -> a:bool):(bool -> bool):(* -> *)
            var expected =
                Lambda(
                    BoundVariable("a", Variable("bool", Kind())),
                    Variable("a", Variable("bool", Kind())),
                    Lambda(
                        Variable("bool", Kind()),
                        Variable("bool", Kind()),
                        Lambda(
                            Kind(),
                            Kind())));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation17()
        {
            var environments = Environments();

            // (a -> a):(_ -> bool):(_ -> *)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a"),
                    Lambda(
                        Unspecified(),
                        Variable("bool"),
                        Lambda(
                            Unspecified(),
                            Kind())));

            var actual = environments.Infer(expression);

            // (a:bool -> a:bool):(bool -> bool):(* -> *)
            var expected =
                Lambda(
                    BoundVariable("a", Variable("bool", Kind())),
                    Variable("a", Variable("bool", Kind())),
                    Lambda(
                        Variable("bool", Kind()),
                        Variable("bool", Kind()),
                        Lambda(
                            Kind(),
                            Kind())));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation18()
        {
            var environments = Environments();

            // (a -> a):(_ -> bool):(* -> _)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a"),
                    Lambda(
                        Unspecified(),
                        Variable("bool"),
                        Lambda(
                            Kind(),
                            Unspecified())));

            var actual = environments.Infer(expression);

            // (a:bool -> a:bool):(bool -> bool):(* -> *)
            var expected =
                Lambda(
                    BoundVariable("a", Variable("bool", Kind())),
                    Variable("a", Variable("bool", Kind())),
                    Lambda(
                        Variable("bool", Kind()),
                        Variable("bool", Kind()),
                        Lambda(
                            Kind(),
                            Kind())));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaWithAnnotation19()
        {
            var environments = Environments();

            // (a -> a):(bool -> _):(_ -> *)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a"),
                    Lambda(
                        Variable("bool"),
                        Unspecified(),
                        Lambda(
                            Unspecified(),
                            Kind())));

            var actual = environments.Infer(expression);

            // (a:bool:* -> a:bool:*):(bool:* -> bool:*):(* -> *)
            var expected =
                Lambda(
                    BoundVariable("a", Variable("bool", Kind())),
                    Variable("a", Variable("bool", Kind())),
                    Lambda(
                        Variable("bool", Kind()),
                        Variable("bool", Kind()),
                        Lambda(
                            Kind(),
                            Kind())));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaShadowedVariable1()
        {
            var environments = Environments();

            // a = c:int
            environments.MutableBind(
                "a",
                Variable("c", Type<int>()));

            // a -> a:bool
            var expression =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("a"));

            var actual = environments.Infer(expression);

            // (a:bool -> a:bool):(bool -> bool)
            var expected =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("a", Type<bool>()),
                    Lambda(Type<bool>(), Type<bool>()));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaShadowedVariable2()
        {
            var environments = Environments();

            // b:int = c
            environments.MutableBind(
                BoundVariable("b", Type<int>()),
                Variable("c"));

            // a:bool -> b
            var expression =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("b"));

            var actual = environments.Infer(expression);

            // (a:bool -> b:int):(bool -> int)
            var expected =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("b", Type<int>()),
                    Lambda(Type<bool>(), Type<int>()));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaShadowedVariable3()
        {
            var environments = Environments();

            // b = 123:int
            environments.MutableBind(
                "b",
                Constant(123));

            // a:bool -> b
            var expression =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("b"));

            var actual = environments.Infer(expression);

            // (a:bool -> b:int):(bool -> int)
            var expected =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("b", Type<int>()),
                    Lambda(Type<bool>(), Type<int>()));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void LambdaComplex1()
        {
            var environments = Environments();

            // b = 123:int
            environments.MutableBind(
                "b",
                Constant(123));

            // a -> b
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("b"));

            var actual = environments.Infer(expression);

            // (a:'0 -> b:int):('0 -> int)
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Lambda(
                    BoundVariable("a", ph0),
                    Variable("b", Type<int>()),
                    Lambda(ph0, Type<int>()));

            AssertLogicalEqual(expression, expected, actual);
        }
 
        [Test]
        public void LambdaOperator1()
        {
            var environments = Environments();

            // -> a a
            var expression =
                Apply(
                    Apply(
                        Variable("->"),
                        Variable("a")),
                    Variable("a"));

            var actual = environments.Infer(expression);

            // (a:'0 -> a:'0):('0 -> '0)
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Lambda(
                    BoundVariable("a", ph0),
                    Variable("a", ph0),
                    Lambda(ph0, ph0));

            AssertLogicalEqual(expression, expected, actual);
        }
    }
}
