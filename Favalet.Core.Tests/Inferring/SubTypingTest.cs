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
    public sealed class SubTypingTest
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
        
        #region Widening
        [Test]
        public void Widening1()
        {
            var environment = CLREnvironments();
            
            // bool || IConvertible
            var expression =
                Or(
                    Type<bool>(),
                    Type<IConvertible>());

            var actual = environment.TypeCalculator.Compute(expression);

            // IConvertible
            var expected =
                Type<IConvertible>();

            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void Widening2()
        {
            var environment = CLREnvironments();

            // bool || IConvertible || int
            var expression =
                Or(
                    Type<bool>(),
                    Or(
                        Type<IConvertible>(),
                        Type<int>()));

            var actual = environment.TypeCalculator.Compute(expression);

            // IConvertible
            var expected =
                Type<IConvertible>();

            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void Widening3()
        {
            var environment = CLREnvironments();

            // IConvertible || bool || int
            var expression =
                Or(
                    Type<IConvertible>(),
                    Or(
                        Type<bool>(),
                        Type<int>()));

            var actual = environment.TypeCalculator.Compute(expression);

            // IConvertible
            var expected =
                Type<IConvertible>();

            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void Widening4()
        {
            var environment = CLREnvironments();

            // bool || IConvertible || int
            var expression =
                Or(
                    Or(
                        Type<bool>(),
                        Type<IConvertible>()),
                    Type<int>());

            var actual = environment.TypeCalculator.Compute(expression);

            // IConvertible
            var expected =
                Type<IConvertible>();

            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void Widening5()
        {
            var environment = CLREnvironments();

            // bool || int || IConvertible
            var expression =
                Or(
                    Or(
                        Type<bool>(),
                        Type<int>()),
                    Type<IConvertible>());

            var actual = environment.TypeCalculator.Compute(expression);

            // IConvertible
            var expected =
                Type<IConvertible>();

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion
        
        #region Narrowing
        [Test]
        public void Narrowing1()
        {
            var environment = CLREnvironments();
            
            // bool && IConvertible
            var expression =
                And(
                    Type<bool>(),
                    Type<IConvertible>());

            var actual = environment.TypeCalculator.Compute(expression);

            // bool
            var expected =
                Type<bool>();

            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void Narrowing2()
        {
            var environment = CLREnvironments();

            // bool && IConvertible && int
            var expression =
                And(
                    Type<bool>(),
                    And(
                        Type<IConvertible>(),
                        Type<int>()));

            var actual = environment.TypeCalculator.Compute(expression);

            // bool && int
            var expected =
                And(
                    Type<bool>(),
                    Type<int>());

            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void Narrowing3()
        {
            var environment = CLREnvironments();

            // IConvertible && bool && int
            var expression =
                And(
                    Type<IConvertible>(),
                    And(
                        Type<bool>(),
                        Type<int>()));

            var actual = environment.TypeCalculator.Compute(expression);

            // bool && int
            var expected =
                And(
                    Type<bool>(),
                    Type<int>());

            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void Narrowing4()
        {
            var environment = CLREnvironments();

            // bool && IConvertible && int
            var expression =
                And(
                    And(
                        Type<bool>(),
                        Type<IConvertible>()),
                    Type<int>());

            var actual = environment.TypeCalculator.Compute(expression);

            // bool && int
            var expected =
                And(
                    Type<bool>(),
                    Type<int>());

            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void Narrowing5()
        {
            var environment = CLREnvironments();

            // bool && int && IConvertible
            var expression =
                And(
                    And(
                        Type<bool>(),
                        Type<int>()),
                    Type<IConvertible>());

            var actual = environment.TypeCalculator.Compute(expression);

            // bool && int
            var expected =
                And(
                    Type<bool>(),
                    Type<int>());

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion

        #region Covariance
        [Test]
        public void CovarianceInLambdaBody1()
        {
            var environment = CLREnvironments();

            // (a -> a):(int -> object)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a"),
                    Function(
                        Type<int>(),
                        Type<object>()));

            var actual = environment.Infer(expression);

            // (a:int -> a:int):(int -> object)
            var expected =
                Lambda(
                    BoundVariable("a", Type<int>()),
                    Variable("a", Type<int>()),
                    Function(
                        Type<int>(),
                        Type<object>()));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void CovarianceInLambdaBody2()
        {
            var environment = CLREnvironments();

            // (a:int -> a):(_ -> object)
            var expression =
                Lambda(
                    BoundVariable("a", Type<int>()),
                    Variable("a"),
                    Function(
                        Unspecified(),
                        Type<object>()));

            var actual = environment.Infer(expression);

            // (a:int -> a:int):(int -> object)
            var expected =
                Lambda(
                    BoundVariable("a", Type<int>()),
                    Variable("a", Type<int>()),
                    Function(
                        Type<int>(),
                        Type<object>()));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void CovarianceInLambdaBody3()
        {
            var environment = CLREnvironments();

            // (a -> a:int):(_ -> object)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a", Type<int>()),
                    Function(
                        Unspecified(),
                        Type<object>()));

            var actual = environment.Infer(expression);

            // (a:int -> a:int):(int -> object)
            var expected =
                Lambda(
                    BoundVariable("a", Type<int>()),
                    Variable("a", Type<int>()),
                    Function(
                        Type<int>(),
                        Type<object>()));

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion

        #region Contravariance
        [Test]
        public void ContravarianceInLambdaBody1()
        {
            var environment = CLREnvironments();

            // (a:object -> a):(int -> object)
            var expression =
                Lambda(
                    BoundVariable("a", Type<object>()),
                    Variable("a"),
                    Function(
                        Type<int>(),
                        Type<object>()));

            var actual = environment.Infer(expression);

            // (a:object -> a:object):(int -> object)
            var expected =
                Lambda(
                    BoundVariable("a", Type<object>()),
                    Variable("a", Type<object>()),
                    Function(
                        Type<int>(),
                        Type<object>()));

            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void ContravarianceInLambdaBody2()
        {
            var environment = CLREnvironments();

            // (a -> a:object):(int -> object)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a", Type<object>()),
                    Function(
                        Type<int>(),
                        Type<object>()));

            var actual = environment.Infer(expression);

            // (a:int -> a:object):(int -> object)
            var expected =
                Lambda(
                    BoundVariable("a", Type<int>()),
                    Variable("a", Type<object>()),
                    Function(
                        Type<int>(),
                        Type<object>()));

            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void ContravarianceInLambdaBody3()
        {
            var environment = CLREnvironments();

            // (a:object -> a):(int -> _)
            var expression =
                Lambda(
                    BoundVariable("a", Type<object>()),
                    Variable("a"),
                    Function(
                        Type<int>(),
                        Unspecified()));

            var actual = environment.Infer(expression);

            // (a:object -> a:object):(int -> object)
            var expected =
                Lambda(
                    BoundVariable("a", Type<object>()),
                    Variable("a", Type<object>()),
                    Function(
                        Type<int>(),
                        Type<object>()));

            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void ContravarianceInLambdaBody4()
        {
            var environment = CLREnvironments();

            // (a -> a:object):(int -> _)
            var expression =
                Lambda(
                    BoundVariable("a"),
                    Variable("a", Type<object>()),
                    Function(
                        Type<int>(),
                        Unspecified()));

            var actual = environment.Infer(expression);

            // (a:int -> a:object):(int -> object)
            var expected =
                Lambda(
                    BoundVariable("a", Type<int>()),
                    Variable("a", Type<object>()),
                    Function(
                        Type<int>(),
                        Type<object>()));

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion
    }
}
