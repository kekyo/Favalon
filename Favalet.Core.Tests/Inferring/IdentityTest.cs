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
using Favalet.Expressions.Specialized;
using static Favalet.CLRGenerator;
using static Favalet.Generator;

namespace Favalet.Inferring
{
    [TestFixture]
    public sealed class IdentityTest
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

        #region Variable
        [Test]
        public void FromVariable1()
        {
            var environment = CLREnvironments();

            environment.MutableBind(
                "true",
                Constant(true));

            // true
            var expression =
                Variable("true");

            var actual = environment.Infer(expression);

            // true:bool
            var expected =
                Variable("true", Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void FromVariable2()
        {
            var environment = CLREnvironments();

            environment.MutableBind(
                "true",
                Constant(true));
            environment.MutableBind(
                "false",
                Constant(false));

            // true && false
            var expression =
                And(
                    Variable("true"),
                    Variable("false"));

            var actual = environment.Infer(expression);

            // (true:bool && false:bool):bool
            var expected =
                And(
                    Variable("true", Type<bool>()),
                    Variable("false", Type<bool>()),
                    Type<bool>());

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion
 
        #region BoundVariable
        [Test]
        public void BoundVariableFixedRelation1()
        {
            var environment = CLREnvironments();

            // (a -> a):(object -> _)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a"),
                    Lambda(
                        Type<object>(),
                        Unspecified()));

            var actual = environment.Infer(expression);

            // (a:object -> a:object):(object -> object)
            var expected =
                Lambda(
                    BoundVariable("a", Type<object>()),
                    Variable("a", Type<object>()),
                    Lambda(
                        Type<object>(),
                        Type<object>()));

            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void BoundVariableFixedRelation3()
        {
            var environment = CLREnvironments();

            // a:object -> a
            var expression =
                Lambda(
                    BoundVariable("a", Type<object>()),
                    Variable("a"));

            var actual = environment.Infer(expression);

            // (a:object -> a:object):(object -> object)
            var expected =
                Lambda(
                    BoundVariable("a", Type<object>()),
                    Variable("a", Type<object>()),
                    Lambda(
                        Type<object>(),
                        Type<object>()));

            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void BoundVariableWithAttributes1()
        {
            var environment = CLREnvironments();
            
            // $$$ @ PREFIX|LTR = a -> a
            environment.MutableBind(
                BoundAttributes.PrefixLeftToRight,
                BoundVariable("$$$"),
                Lambda(
                    "a",
                    Variable("a")));

            // $$$ 123
            var expression =
                Apply(
                    Variable("$$$"),
                    Constant(123));

            var actual = environment.Infer(expression);

            // ($$$:(int -> int) 123:int):int
            var expected =
                Apply(
                    Variable("$$$",
                        Lambda(
                            Type<int>(),
                            Type<int>())),
                    Constant(123),
                    Type<int>());

            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void BoundVariableWithAttributes2()
        {
            var environment = CLREnvironments();
            
            // $$$ @ INFIX|LTR = a -> a
            environment.MutableBind(
                BoundAttributes.InfixLeftToRight,
                BoundVariable("$$$"),
                Lambda(
                    "a",
                    Variable("a")));

            // 123 $$$
            var expression =
                Apply(
                    Constant(123),
                    Variable("$$$"));

            var actual = environment.Infer(expression);

            // ($$$:(int -> int) 123:int):int
            var expected =
                Apply(
                    Variable("$$$",
                        Lambda(
                            Type<int>(),
                            Type<int>())),
                    Constant(123),
                    Type<int>());

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion
 
        #region TypeVariable
        [Test]
        public void TypeVariable1()
        {
            var environment = CLREnvironments();

            // a:bool -> a
            var expression =
                Lambda(
                    BoundVariable("a", Variable("bool")),
                    Variable("a"));

            var actual = environment.Infer(expression);

            // a:bool -> a:bool
            var expected =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("a", Type<bool>()));

            AssertLogicalEqual(expression, expected, actual);
        }
 
        [Test]
        public void TypeVariable2()
        {
            var environment = CLREnvironments();

            // a -> a:bool
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a", Type<bool>()));

            var actual = environment.Infer(expression);

            // a:bool -> a:bool
            var expected =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("a", Type<bool>()));

            AssertLogicalEqual(expression, expected, actual);
        }
 
        [Test]
        public void TypeVariable3()
        {
            var environment = CLREnvironments();

            // (a -> a):(bool -> _)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a"),
                    Lambda(
                        Variable("bool"),
                        Unspecified()));

            var actual = environment.Infer(expression);

            // (a:bool -> a:bool):(bool -> bool)
            var expected =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("a", Type<bool>()),
                    Lambda(
                        Type<bool>(),
                        Type<bool>()));

            AssertLogicalEqual(expression, expected, actual);
        }
 
        [Test]
        public void TypeVariable4()
        {
            var environment = CLREnvironments();

            // (a -> a):(_ -> bool)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a"),
                    Lambda(
                        Unspecified(),
                        Variable("bool")));

            var actual = environment.Infer(expression);

            // (a:bool -> a:bool):(bool -> bool)
            var expected =
                Lambda(
                    BoundVariable("a", Type<bool>()),
                    Variable("a", Type<bool>()),
                    Lambda(
                        Type<bool>(),
                        Type<bool>()));

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion
    }
}
