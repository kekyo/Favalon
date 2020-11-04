using System;
using Favalet.Contexts;
using Favalet.Expressions;
using NUnit.Framework;

using static Favalet.CLRGenerator;
using static Favalet.Generator;

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
            var environment = CLREnvironment();
            
            // a = 123
            // a = 123.456
            // a = x -> x
            environment.MutableBind("a", Constant(123));
            environment.MutableBind("a", Constant(123.456));
            environment.MutableBind("a",
                Lambda(
                    "x",
                    Variable("x")));

            // a:int
            var expression =
                Variable("a", Type<int>());

            var actual = environment.Infer(expression);

            // a:int
            var expected =
                Variable("a", Type<int>());

            AssertLogicalEqual(expression, expected, actual);
        }
                
        [Test]
        public void OverloadingExactMatch2()
        {
            var environment = CLREnvironment();
            
            // a = 123
            // a = 123.456
            // a = x -> x
            environment.MutableBind("a", Constant(123));
            environment.MutableBind("a", Constant(123.456));
            environment.MutableBind("a",
                Lambda(
                    "x",
                    Variable("x")));

            // a:double
            var expression =
                Variable("a", Type<double>());

            var actual = environment.Infer(expression);

            // a:double
            var expected =
                Variable("a", Type<double>());

            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void OverloadingExactMatch3()
        {
            var environment = CLREnvironment();
            
            // a = [object]
            // a = 123
            environment.MutableBind("a", Constant(new object()));
            environment.MutableBind("a", Constant(123));

            // a:int
            var expression =
                Variable("a", Type<int>());

            var actual = environment.Infer(expression);

            // a:int
            var expected =
                Variable("a", Type<int>());

            AssertLogicalEqual(expression, expected, actual);
        }
                    
        [Test]
        public void OverloadingExactMatch4()
        {
            var environment = CLREnvironment();
            
            // a = [object]
            // a = 123
            environment.MutableBind("a", Constant(new object()));
            environment.MutableBind("a", Constant(123));

            // a:object
            var expression =
                Variable("a", Type<object>());

            var actual = environment.Infer(expression);

            // a:object
            var expected =
                Variable("a", Type<object>());

            AssertLogicalEqual(expression, expected, actual);
        }
                    
        [Test]
        public void OverloadingExactMatch5()
        {
            var environment = CLREnvironment();
            
            // a = [object]
            // a = 123
            environment.MutableBind("a", Constant(new object()));
            environment.MutableBind("a", Constant(123));

            // a:IFormattable
            var expression =
                Variable("a", Type<IFormattable>());

            var actual = environment.Infer(expression);

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
            var environment = CLREnvironment();
            
            // a = 123
            // a = 123.456
            // a = x -> x
            environment.MutableBind("a", Constant(123));
            environment.MutableBind("a", Constant(123.456));
            environment.MutableBind("a",
                Lambda(
                    "x",
                    Variable("x")));

            // a:(_ -> _))
            var expression =
                Variable("a",
                    Function(
                        Unspecified(),
                        Unspecified()));

            var actual = environment.Infer(expression);

            // a:('0 -> '0)
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Variable("a",
                    Function(
                        ph0,
                        ph0));

            AssertLogicalEqual(expression, expected, actual);
        }
                
        [Test]
        public void OverloadingFunctionExactMatch2()
        {
            var environment = CLREnvironment();
            
            // a = 123
            // a = 123.456
            // a = x -> x
            environment.MutableBind("a", Constant(123));
            environment.MutableBind("a", Constant(123.456));
            environment.MutableBind("a",
                Lambda(
                    "x",
                    Variable("x")));

            // a:(b -> b))
            var expression =
                Variable("a",
                    Function(
                        Variable("b"),
                        Variable("b")));

            var actual = environment.Infer(expression);

            // a:('0 -> '0)
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Variable("a",
                    Function(
                        ph0,
                        ph0));

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion

        #region Indirect match
        [Test]
        public void ApplyOverloadingExactMatch1()
        {
            var environment = CLREnvironment();
            
            // a = 123
            // a = 123.456
            // a = x -> x
            environment.MutableBind("a", Constant(123));
            environment.MutableBind("a", Constant(123.456));
            environment.MutableBind("a",
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

            var actual = environment.Infer(expression);

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
            var environment = CLREnvironment();
            
            // a = 123
            // a = 123.456
            // a = x -> x
            environment.MutableBind("a", Constant(123));
            environment.MutableBind("a", Constant(123.456));
            environment.MutableBind("a",
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

            var actual = environment.Infer(expression);

            // (x:double -> x:double) a:double
            var expected =
                Apply(
                    Lambda(
                        BoundVariable("x", Type<double>()),
                        Variable("x", Type<double>())),
                    Variable("a", Type<double>()));

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion
    }
}
