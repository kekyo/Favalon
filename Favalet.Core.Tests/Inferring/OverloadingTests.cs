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

using System;
using Favalet.Contexts;
using Favalet.Expressions;
using NUnit.Framework;

using static Favalet.CLRGenerator;
using static Favalet.Generator;
using static Favalet.TestUtiltiies;

namespace Favalet.Inferring
{
    [TestFixture]
    public sealed class OverloadingTest
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
        
        #region Exact match
        [Test]
        public void OverloadingExactMatch1()
        {
            var environments = CLREnvironments();
            
            // a = 123
            // a = 123.456
            // a = x -> x
            environments.MutableBind("a", Constant(123));
            environments.MutableBind("a", Constant(123.456));
            environments.MutableBind("a",
                Lambda(
                    "x",
                    Variable("x")));

            // a:int
            var expression =
                Variable("a", Type<int>());

            var actual = environments.Infer(expression);

            // a:int
            var expected =
                Variable("a", Type<int>());

            AssertLogicalEqual(expression, expected, actual);
        }
                
        [Test]
        public void OverloadingExactMatch2()
        {
            var environments = CLREnvironments();
            
            // a = 123
            // a = 123.456
            // a = x -> x
            environments.MutableBind("a", Constant(123));
            environments.MutableBind("a", Constant(123.456));
            environments.MutableBind("a",
                Lambda(
                    "x",
                    Variable("x")));

            // a:double
            var expression =
                Variable("a", Type<double>());

            var actual = environments.Infer(expression);

            // a:double
            var expected =
                Variable("a", Type<double>());

            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void OverloadingExactMatch3()
        {
            var environments = CLREnvironments();
            
            // a = [object]
            // a = 123
            environments.MutableBind("a", Constant(new object()));
            environments.MutableBind("a", Constant(123));

            // a:int
            var expression =
                Variable("a", Type<int>());

            var actual = environments.Infer(expression);

            // a:int
            var expected =
                Variable("a", Type<int>());

            AssertLogicalEqual(expression, expected, actual);
        }
                    
        [Test]
        public void OverloadingExactMatch4()
        {
            var environments = CLREnvironments();
            
            // a = [object]
            // a = 123
            environments.MutableBind("a", Constant(new object()));
            environments.MutableBind("a", Constant(123));

            // a:object
            var expression =
                Variable("a", Type<object>());

            var actual = environments.Infer(expression);

            // a:object
            var expected =
                Variable("a", Type<object>());

            AssertLogicalEqual(expression, expected, actual);
        }
                    
        [Test]
        public void OverloadingExactMatch5()
        {
            var environments = CLREnvironments();
            
            // a = [object]
            // a = 123
            environments.MutableBind("a", Constant(new object()));
            environments.MutableBind("a", Constant(123));

            // a:IFormattable
            var expression =
                Variable("a", Type<IFormattable>());

            var actual = environments.Infer(expression);

            // a:IFormattable
            var expected =
                Variable("a", Type<IFormattable>());

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion
                
        #region Function exact match
        [Test]
        public void OverloadingFunctionExactMatch1()
        {
            var environments = CLREnvironments();
            
            // a = 123
            // a = 123.456
            // a = x -> x
            environments.MutableBind("a", Constant(123));
            environments.MutableBind("a", Constant(123.456));
            environments.MutableBind("a",
                Lambda(
                    "x",
                    Variable("x")));

            // a:(_ -> _))
            var expression =
                Variable("a",
                    Lambda(
                        Unspecified(),
                        Unspecified()));

            var actual = environments.Infer(expression);

            // a:('0 -> '0)
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Variable("a",
                    Lambda(
                        ph0,
                        ph0));

            AssertLogicalEqual(expression, expected, actual);
        }
                
        [Test]
        public void OverloadingFunctionExactMatch2()
        {
            var environments = CLREnvironments();
            
            // a = 123
            // a = 123.456
            // a = x -> x
            environments.MutableBind("a", Constant(123));
            environments.MutableBind("a", Constant(123.456));
            environments.MutableBind("a",
                Lambda(
                    "x",
                    Variable("x")));

            // a:(b -> b))
            var expression =
                Variable("a",
                    Lambda(
                        Variable("b"),
                        Variable("b")));

            var actual = environments.Infer(expression);

            // a:('0 -> '0)
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Variable("a",
                    Lambda(
                        ph0,
                        ph0));

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion

        #region Indirect match
        [Test]
        public void ApplyOverloadingExactMatch1()
        {
            var environments = CLREnvironments();
            
            // a = 123
            // a = 123.456
            // a = x -> x
            environments.MutableBind("a", Constant(123));
            environments.MutableBind("a", Constant(123.456));
            environments.MutableBind("a",
                Lambda(
                    "x",
                    Variable("x")));

            // (x:int -> x) a
            var expression =
                Apply(
                    Lambda(
                        BoundVariable("x", Type<int>()),
                        Variable("x")),
                    Variable("a"));

            var actual = environments.Infer(expression);

            // (x:int -> x:int) a:int
            var expected =
                Apply(
                    Lambda(
                        BoundVariable("x", Type<int>()),
                        Variable("x", Type<int>())),
                    Variable("a", Type<int>()));

            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void ApplyOverloadingExactMatch2()
        {
            var environments = CLREnvironments();
            
            // a = 123
            // a = 123.456
            // a = x -> x
            environments.MutableBind("a", Constant(123));
            environments.MutableBind("a", Constant(123.456));
            environments.MutableBind("a",
                Lambda(
                    "x",
                    Variable("x")));

            // (x:double -> x) a
            var expression =
                Apply(
                    Lambda(
                        BoundVariable("x", Type<double>()),
                        Variable("x")),
                    Variable("a"));

            var actual = environments.Infer(expression);

            // (x:double -> x:double) a:double
            var expected =
                Apply(
                    Lambda(
                        BoundVariable("x", Type<double>()),
                        Variable("x", Type<double>())),
                    Variable("a", Type<double>()));

            AssertLogicalEqual(expression, expected, actual);
        }
        
        //[Test]
        public void ApplyComplexOverloadingMatch1()
        {
            var environments = CLREnvironments();

            // System.Convert.ToInt32 "123"
            var expression =
                Apply(
                    Variable("System.Convert.ToInt32"),
                    Constant("123"));

            var actual = environments.Infer(expression);

            // (System.Convert.ToInt32:((string -> int) || (string -> IFormatProvider -> int)) "123":string):(int || (IFormatProvider -> int))
            var expected =
                Apply(
                    Variable(
                        "System.Convert.ToInt32",
                        Or(
                            Lambda(Type<string>(), Type<int>()),
                            Lambda(Type<string>(), Lambda(Type<IFormatProvider>(), Type<int>())))),
                    Constant("123"),
                    Type<int>());

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion

        #region From delegate
        [Test]
        public void StaticMethodDelegate()
        {
            var environments = CLREnvironments();

            // int.Parse
            var expression =
                Delegate<string, int>(int.Parse);

            var actual = environments.Infer(expression);

            // int.Parse:(string -> int)
            var expectedHigherOrder =
                Lambda(
                    Type<string>(),
                    Type<int>());

            AssertLogicalEqual(expression, expectedHigherOrder, actual.HigherOrder);
        }
        
        [Test]
        public void InstanceMethodDelegate()
        {
            var environments = CLREnvironments();

            // DateTime.Now.Add
            var now = DateTime.Now;
            var expression =
                Delegate<TimeSpan, DateTime>(now.Add);

            var actual = environments.Infer(expression);

            // DateTime.Add:(TimeSpan -> DateTime)
            var expectedHigherOrder =
                Lambda(
                    Type<TimeSpan>(),
                    Type<DateTime>());

            AssertLogicalEqual(expression, expectedHigherOrder, actual.HigherOrder);
        }
        
        [Test]
        public void ExtensionMethodDelegate()
        {
            var environments = CLREnvironments();

            // uri.UriMethod
            var uri = new Uri("https://example.com/", UriKind.RelativeOrAbsolute);
            var expression =
                Delegate<int, string>(uri.UriMethod);

            var actual = environments.Infer(expression);

            // <closure>:(int -> string)
            var expectedHigherOrder =
                Lambda(
                    Type<int>(),
                    Type<string>());

            AssertLogicalEqual(expression, expectedHigherOrder, actual.HigherOrder);
        }
        #endregion
    }

    internal static class ExtensionMethodDelegateTest
    {
        public static string UriMethod(this Uri url, int number) =>
            url.ToString() + number;
    }
}
