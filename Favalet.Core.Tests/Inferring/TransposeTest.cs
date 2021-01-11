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

using static Favalet.CLRGenerator;
using static Favalet.Generator;
using static Favalet.TestUtiltiies;

namespace Favalet.Inferring
{
    [TestFixture]
    public sealed class TransposeTest
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
 
        ////////////////////////////////////////////////////////////////////////////////////

        [Test]
        public void InfixEqualPrecedence1()
        {
            var environment = Environments();
            
            // *
            environment.MutableBind(
                BoundVariable("*", InfixLeftToRight(BoundPrecedences.Multiply)),
                Lambda("a", Lambda("b", Constant(123))));
            // **
            environment.MutableBind(
                BoundVariable("**", InfixLeftToRight(BoundPrecedences.Multiply)),
                Lambda("a", Lambda("b", Constant(456))));

            // 1 * 2 ** 3
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Constant(1),
                                Variable("*")),
                            Constant(2)),
                        Variable("**")),
                    Constant(3));

            var actual = environment.Infer(expression);

            // (** ((* 1) 2)) 3
            var expected =
                Apply(
                    Apply(
                        Variable("**"),
                        Apply(
                            Apply(
                                Variable("*"),
                                Constant(1)),
                            Constant(2))),
                    Constant(3));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void InfixEqualPrecedence2()
        {
            var environment = Environments();
            
            // +
            environment.MutableBind(
                BoundVariable("+", InfixLeftToRight(BoundPrecedences.Multiply)),
                Lambda("a", Lambda("b", Constant(123))));
            // *
            environment.MutableBind(
                BoundVariable("*", InfixLeftToRight(BoundPrecedences.Multiply)),
                Lambda("a", Lambda("b", Constant(456))));
            // @
            environment.MutableBind(
                BoundVariable("@", InfixLeftToRight(BoundPrecedences.Multiply)),
                Lambda("a", Lambda("b", Constant(789))));

            // 1 * 2 + 3 * 4
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Apply(
                                    Apply(
                                        Constant(1),
                                        Variable("*")),
                                    Constant(2)),
                                Variable("+")),
                            Constant(3)),
                        Variable("@")),
                    Constant(4));

            var actual = environment.Infer(expression);

            // (@ ((+ ((* 1) 2)) 3)) 4
            var expected =
                Apply(
                    Apply(
                        Variable("@"),
                        Apply(
                            Apply(
                                Variable("+"),
                                Apply(
                                    Apply(
                                        Variable("*"),
                                        Constant(1)),
                                    Constant(2))),
                            Constant(3))),
                    Constant(4));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void InfixHigherToLowerPrecedence()
        {
            var environment = Environments();
            
            // +
            environment.MutableBind(
                BoundVariable("+", InfixLeftToRight(BoundPrecedences.Addition)),
                Lambda("a", Lambda("b", Constant(123))));
            // *
            environment.MutableBind(
                BoundVariable("*", InfixLeftToRight(BoundPrecedences.Multiply)),
                Lambda("a", Lambda("b", Constant(456))));

            // 1 * 2 + 3
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Constant(1),
                                Variable("*")),
                            Constant(2)),
                        Variable("+")),
                    Constant(3));

            var actual = environment.Infer(expression);

            // (+ ((* 1) 2) 3
            var expected =
                Apply(
                    Apply(
                        Variable("+"),
                        Apply(
                            Apply(
                                Variable("*"),
                                Constant(1)),
                            Constant(2))),
                    Constant(3));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void InfixLowerToHigherPrecedence()
        {
            var environment = Environments();
            
            // +
            environment.MutableBind(
                BoundVariable("+", InfixLeftToRight(BoundPrecedences.Addition)),
                Lambda("a", Lambda("b", Constant(123))));
            // *
            environment.MutableBind(
                BoundVariable("*", InfixLeftToRight(BoundPrecedences.Multiply)),
                Lambda("a", Lambda("b", Constant(456))));

            // 1 + 2 * 3
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Constant(1),
                                Variable("+")),
                            Constant(2)),
                        Variable("*")),
                    Constant(3));

            var actual = environment.Infer(expression);

            // (+ 1) ((* 2) 3)
            var expected =
                Apply(
                    Apply(
                        Variable("+"),
                        Constant(1)),
                    Apply(
                        Apply(
                            Variable("*"),
                            Constant(2)),
                        Constant(3)));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void InfixHigherToLowerToHigherPrecedence()
        {
            var environment = Environments();
            
            // +
            environment.MutableBind(
                BoundVariable("+", InfixLeftToRight(BoundPrecedences.Addition)),
                Lambda("a", Lambda("b", Constant(123))));
            // *
            environment.MutableBind(
                BoundVariable("*", InfixLeftToRight(BoundPrecedences.Multiply)),
                Lambda("a", Lambda("b", Constant(456))));

            // 1 * 2 + 3 * 4
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Apply(
                                    Apply(
                                        Constant(1),
                                        Variable("*")),
                                    Constant(2)),
                                Variable("+")),
                            Constant(3)),
                        Variable("*")),
                    Constant(4));

            var actual = environment.Infer(expression);

            // (+ ((* 1) 2)) ((* 3) 4)
            var expected =
                Apply(
                    Apply(
                        Variable("+"),
                        Apply(
                            Apply(
                                Variable("*"),
                                Constant(1)),
                            Constant(2))),
                    Apply(
                        Apply(
                            Variable("*"),
                            Constant(3)),
                        Constant(4)));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void InfixLowerToHigherToLowerPrecedence()
        {
            var environment = Environments();
            
            // +
            environment.MutableBind(
                BoundVariable("+", InfixLeftToRight(BoundPrecedences.Addition)),
                Lambda("a", Lambda("b", Constant(123))));
            // *
            environment.MutableBind(
                BoundVariable("*", InfixLeftToRight(BoundPrecedences.Multiply)),
                Lambda("a", Lambda("b", Constant(456))));

            // 1 + 2 * 3 + 4
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Apply(
                                    Apply(
                                        Constant(1),
                                        Variable("+")),
                                    Constant(2)),
                                Variable("*")),
                            Constant(3)),
                        Variable("+")),
                    Constant(4));

            var actual = environment.Infer(expression);

            // (+ ((+ 1) ((* 2) 3))) 4
            var expected =
                Apply(
                    Apply(
                        Variable("+"),
                        Apply(
                            Apply(
                                Variable("+"),
                                Constant(1)),
                            Apply(
                                Apply(
                                    Variable("*"),
                                    Constant(2)),
                                Constant(3)))),
                    Constant(4));

            AssertLogicalEqual(expression, expected, actual);
        }
 
        [Test]
        public void InfixHigherToMiddleToLowerPrecedence()
        {
            var environment = Environments();
            
            // +
            environment.MutableBind(
                BoundVariable("+", InfixLeftToRight(BoundPrecedences.Addition)),
                Lambda("a", Lambda("b", Constant(123))));
            // *
            environment.MutableBind(
                BoundVariable("*", InfixLeftToRight(BoundPrecedences.Multiply)),
                Lambda("a", Lambda("b", Constant(456))));
            // @
            environment.MutableBind(
                BoundVariable("@", InfixLeftToRight(-950)),
                Lambda("a", Lambda("b", Constant(789))));

            // 1 * 2 @ 3 + 4
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Apply(
                                    Apply(
                                        Constant(1),
                                        Variable("*")),
                                    Constant(2)),
                                Variable("@")),
                            Constant(3)),
                        Variable("+")),
                    Constant(4));

            var actual = environment.Infer(expression);

            // (+ ((@ ((* 1) 2)) 3)) 4
            var expected =
                Apply(
                    Apply(
                        Variable("+"),
                        Apply(
                            Apply(
                                Variable("@"),
                                Apply(
                                    Apply(
                                        Variable("*"),
                                        Constant(1)),
                                    Constant(2))),
                            Constant(3))),
                    Constant(4));

            AssertLogicalEqual(expression, expected, actual);
        }
 
        [Test]
        public void InfixLowerToMiddleToHigherPrecedence()
        {
            var environment = Environments();
            
            // +
            environment.MutableBind(
                BoundVariable("+", InfixLeftToRight(BoundPrecedences.Addition)),
                Lambda("a", Lambda("b", Constant(123))));
            // *
            environment.MutableBind(
                BoundVariable("*", InfixLeftToRight(BoundPrecedences.Multiply)),
                Lambda("a", Lambda("b", Constant(456))));
            // @
            environment.MutableBind(
                BoundVariable("@", InfixLeftToRight(-950)),
                Lambda("a", Lambda("b", Constant(789))));

            // 1 + 2 @ 3 * 4
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Apply(
                                    Apply(
                                        Constant(1),
                                        Variable("+")),
                                    Constant(2)),
                                Variable("@")),
                            Constant(3)),
                        Variable("*")),
                    Constant(4));

            var actual = environment.Infer(expression);

            // (+ 1) ((@ 2) ((* 3) 4))
            var expected =
                Apply(
                    Apply(
                        Variable("+"),
                        Constant(1)),
                    Apply(
                        Apply(
                            Variable("@"),
                            Constant(2)),
                        Apply(
                            Apply(
                                Variable("*"),
                                Constant(3)),
                            Constant(4))));

            AssertLogicalEqual(expression, expected, actual);
        }
 
        [Test]
        public void InfixHigherToLowerToMiddlePrecedence()
        {
            var environment = Environments();
            
            // +
            environment.MutableBind(
                BoundVariable("+", InfixLeftToRight(BoundPrecedences.Addition)),
                Lambda("a", Lambda("b", Constant(123))));
            // *
            environment.MutableBind(
                BoundVariable("*", InfixLeftToRight(BoundPrecedences.Multiply)),
                Lambda("a", Lambda("b", Constant(456))));
            // @
            environment.MutableBind(
                BoundVariable("@", InfixLeftToRight(-950)),
                Lambda("a", Lambda("b", Constant(789))));

            // 1 * 2 + 3 @ 4
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Apply(
                                    Apply(
                                        Constant(1),
                                        Variable("*")),
                                    Constant(2)),
                                Variable("+")),
                            Constant(3)),
                        Variable("@")),
                    Constant(4));

            var actual = environment.Infer(expression);

            // (+ ((* 1) 2)) ((@ 3) 4)
            var expected =
                Apply(
                    Apply(
                        Variable("+"),
                        Apply(
                            Apply(
                                Variable("*"),
                                Constant(1)),
                            Constant(2))),
                    Apply(
                        Apply(
                            Variable("@"),
                            Constant(3)),
                        Constant(4)));

            AssertLogicalEqual(expression, expected, actual);
        }
 
        [Test]
        public void InfixLowerToHigherToMiddlePrecedence()
        {
            var environment = Environments();
            
            // +
            environment.MutableBind(
                BoundVariable("+", InfixLeftToRight(BoundPrecedences.Addition)),
                Lambda("a", Lambda("b", Constant(123))));
            // *
            environment.MutableBind(
                BoundVariable("*", InfixLeftToRight(BoundPrecedences.Multiply)),
                Lambda("a", Lambda("b", Constant(456))));
            // @
            environment.MutableBind(
                BoundVariable("@", InfixLeftToRight(-950)),
                Lambda("a", Lambda("b", Constant(789))));

            // 1 + 2 * 3 @ 4
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Apply(
                                    Apply(
                                        Constant(1),
                                        Variable("+")),
                                    Constant(2)),
                                Variable("*")),
                            Constant(3)),
                        Variable("@")),
                    Constant(4));

            var actual = environment.Infer(expression);

            // (+ 1) ((@ ((* 2) 3)) 4)
            var expected =
                Apply(
                    Apply(
                        Variable("+"),
                        Constant(1)),
                    Apply(
                        Apply(
                            Variable("@"),
                            Apply(
                                Apply(
                                    Variable("*"),
                                    Constant(2)),
                                Constant(3))),
                        Constant(4)));

            AssertLogicalEqual(expression, expected, actual);
        }
 
        [Test]
        public void InfixMiddleToLowerToHigherPrecedence()
        {
            var environment = Environments();
            
            // +
            environment.MutableBind(
                BoundVariable("+", InfixLeftToRight(BoundPrecedences.Addition)),
                Lambda("a", Lambda("b", Constant(123))));
            // *
            environment.MutableBind(
                BoundVariable("*", InfixLeftToRight(BoundPrecedences.Multiply)),
                Lambda("a", Lambda("b", Constant(456))));
            // @
            environment.MutableBind(
                BoundVariable("@", InfixLeftToRight(-950)),
                Lambda("a", Lambda("b", Constant(789))));

            // 1 @ 2 + 3 * 4
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Apply(
                                    Apply(
                                        Constant(1),
                                        Variable("@")),
                                    Constant(2)),
                                Variable("+")),
                            Constant(3)),
                        Variable("*")),
                    Constant(4));

            var actual = environment.Infer(expression);

            // (+ ((@ 1) 2)) ((* 3) 4)
            var expected =
                Apply(
                    Apply(
                        Variable("+"),
                        Apply(
                            Apply(
                                Variable("@"),
                                Constant(1)),
                            Constant(2))),
                    Apply(
                        Apply(
                            Variable("*"),
                            Constant(3)),
                        Constant(4)));

            AssertLogicalEqual(expression, expected, actual);
        }
 
        [Test]
        public void InfixMiddleToHigherToLowerPrecedence()
        {
            var environment = Environments();
            
            // +
            environment.MutableBind(
                BoundVariable("+", InfixLeftToRight(BoundPrecedences.Addition)),
                Lambda("a", Lambda("b", Constant(123))));
            // *
            environment.MutableBind(
                BoundVariable("*", InfixLeftToRight(BoundPrecedences.Multiply)),
                Lambda("a", Lambda("b", Constant(456))));
            // @
            environment.MutableBind(
                BoundVariable("@", InfixLeftToRight(-950)),
                Lambda("a", Lambda("b", Constant(789))));

            // 1 @ 2 * 3 + 4
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Apply(
                                    Apply(
                                        Constant(1),
                                        Variable("@")),
                                    Constant(2)),
                                Variable("*")),
                            Constant(3)),
                        Variable("+")),
                    Constant(4));

            var actual = environment.Infer(expression);

            // (+ ((@ 1) ((* 2) 3))) 4
            var expected =
                Apply(
                    Apply(
                        Variable("+"),
                        Apply(
                            Apply(
                                Variable("@"),
                                Constant(1)),
                            Apply(
                                Apply(
                                    Variable("*"),
                                    Constant(2)),
                                Constant(3)))),
                    Constant(4));

            AssertLogicalEqual(expression, expected, actual);
        }
    }
}
