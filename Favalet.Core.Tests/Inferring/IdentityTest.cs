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
using Favalet.Expressions.Algebraic;
using NUnit.Framework;

using static Favalet.CLRGenerator;
using static Favalet.Generator;
using static Favalet.TestUtiltiies;

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
 
        ////////////////////////////////////////////////////////////////////////////////////

        #region Variable
        [Test]
        public void FromVariable1()
        {
            var environment = CLREnvironments();

            environment.MutableBind(
                "true",
                BoundAttributes.Neutral,
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
                BoundAttributes.Neutral,
                Constant(true));
            environment.MutableBind(
                "false",
                BoundAttributes.Neutral,
                Constant(false));

            // true && false
            var expression =
                And(
                    Variable("true"),
                    Variable("false"));

            var actual = environment.Infer(expression);

            // (true:bool && false:bool):bool
            var expected =
                AndExpression.UnsafeCreate(
                    Variable("true", Type<bool>()),
                    Variable("false", Type<bool>()),
                    Type<bool>(),
                    Ranges.TextRange.Unknown);

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion
  
        ////////////////////////////////////////////////////////////////////////////////////

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
        #endregion
         
        ////////////////////////////////////////////////////////////////////////////////////

        #region Attributes (Precedences)
        [Test]
        public void BoundVariableWithPrecedences1()
        {
            var environment = CLREnvironments();
            
            // $$$ @ PREFIX|LTR|-10000 = a -> a
            environment.MutableBind(
                BoundVariable("$$$", Prefix(-10000)),
                Lambda(
                    "a",
                    Variable("a")));

            // echo "abc" $$$ 123
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Variable("echo"),
                            Constant("abc")),
                        Variable("$$$")),
                    Constant(123));

            var actual = environment.Infer(expression);

            // ((echo "abc") $$$) 123
            var expected =
                Apply(
                    Apply(
                        Apply(
                            Variable("echo"),
                            Constant("abc")),
                        Variable("$$$")),
                    Constant(123));

            AssertLogicalEqual(expression, expected, actual);
        }
        
        /*
        [Test]
        public void BoundVariableWithPrecedences2()
        {
            var environment = CLREnvironments();
            
            // $$$ @ PREFIX|RTL|-10000 = a -> a
            environment.MutableBind(
                BoundVariable("$$$", PrefixRightToLeft(-10000)),
                Lambda(
                    "a",
                    Variable("a")));

            // echo "abc" $$$ 123
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Variable("echo"),
                            Constant("abc")),
                        Variable("$$$")),
                    Constant(123));

            // TODO: cause error?
            var actual = environment.Infer(expression);

            // ((echo "abc") $$$) 123
            var expected =
                Apply(
                    Apply(
                        Apply(
                            Variable("echo"),
                            Constant("abc")),
                        Variable("$$$")),
                    Constant(123));

            AssertLogicalEqual(expression, expected, actual);
        }
        */
        
        [Test]
        public void BoundVariableWithPrecedences3()
        {
            var environment = CLREnvironments();
            
            // $$$ @ INFIX|LTR|-10000 = a -> a
            environment.MutableBind(
                BoundVariable("$$$", InfixLeftToRight(-10000)),
                Lambda(
                    "a",
                    Variable("a")));

            // echo "abc" $$$ 123
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Variable("echo"),
                            Constant("abc")),
                        Variable("$$$")),
                    Constant(123));

            var actual = environment.Infer(expression);

            // ($$$ (echo "abc")) 123
            var expected =
                Apply(
                    Apply(
                        Variable("$$$"),
                        Apply(
                            Variable("echo"),
                            Constant("abc"))),
                    Constant(123));

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion
        
        #region Attributes (Positions and associativities)
        [Test]
        public void BoundVariableWithPrefixAttributes1()
        {
            var environment = CLREnvironments();
            
            // $$$ @ PREFIX|LTR = a -> a
            environment.MutableBind(
                BoundVariable("$$$", Prefix()),
                Lambda(
                    "a",
                    Variable("a")));

            // $$$ 123
            var expression =
                Apply(
                    Variable("$$$"),
                    Constant(123));

            var actual = environment.Infer(expression);

            // $$$ 123
            var expected =
                Apply(
                    Variable("$$$"),
                    Constant(123));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void BoundVariableWithPrefixAttributes2()
        {
            var environment = CLREnvironments();
            
            // $$$ @ PREFIX|LTR = a -> a
            environment.MutableBind(
                BoundVariable("$$$", Prefix()),
                Lambda(
                    "a",
                    Variable("a")));

            // abc 123 $$$ 456
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Variable("abc"),
                            Constant(123)),
                        Variable("$$$")),
                    Constant(456));
            
            var actual = environment.Infer(expression);

            // ((abc 123) $$$) 456
            var expected =
                Apply(
                    Apply(
                        Apply(
                            Variable("abc"),
                            Constant(123)),
                        Variable("$$$")),
                    Constant(456));
            
            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void BoundVariableWithPrefixAttributes3()
        {
            var environment = CLREnvironments();
            
            // $$$ @ PREFIX|LTR = a -> a
            environment.MutableBind(
                BoundVariable("$$$", Prefix()),
                Lambda(
                    "a",
                    Variable("a")));

            // abc 123 $$$ 456 $$$
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Variable("abc"),
                                Constant(123)),
                            Variable("$$$")),
                        Constant(456)),
                    Variable("$$$"));
            
            var actual = environment.Infer(expression);

            // (((abc 123) $$$) 456) $$$
            var expected =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Variable("abc"),
                                Constant(123)),
                            Variable("$$$")),
                        Constant(456)),
                    Variable("$$$"));
            
            AssertLogicalEqual(expression, expected, actual);
        }

        //////////////////////////////////////////////////////////
         
        [Test]
        public void BoundVariableWithInfixAttributes1()
        {
            var environment = CLREnvironments();
            
            // $$$ @ INFIX|LTR = a -> a
            environment.MutableBind(
                BoundVariable("$$$", InfixLeftToRight()),
                Lambda(
                    "a",
                    Variable("a")));

            // 123 $$$
            var expression =
                Apply(
                    Constant(123),
                    Variable("$$$"));

            var actual = environment.Infer(expression);

            // $$$ 123
            var expected =
                Apply(
                    Variable("$$$"),
                    Constant(123));

            AssertLogicalEqual(expression, expected, actual);
        }
         
        [Test]
        public void BoundVariableWithInfixAttributes2()
        {
            var environment = CLREnvironments();
            
            // $$$ @ INFIX|LTR = a -> a
            environment.MutableBind(
                BoundVariable("$$$", InfixLeftToRight()),
                Lambda(
                    "a",
                    Variable("a")));

            // abc 123 $$$ 456
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Variable("abc"),
                            Constant(123)),
                        Variable("$$$")),
                    Constant(456));
            
            var actual = environment.Infer(expression);

            // ($$$ (abc 123)) 456
            var expected =
                Apply(
                    Apply(
                        Variable("$$$"),
                        Apply(
                            Variable("abc"),
                            Constant(123))),
                    Constant(456));
            
            AssertLogicalEqual(expression, expected, actual);
        }
         
        [Test]
        public void BoundVariableWithInfixAttributes3()
        {
            var environment = CLREnvironments();
            
            // $$$ @ INFIX|LTR = a -> a
            environment.MutableBind(
                BoundVariable("$$$", InfixLeftToRight()),
                Lambda(
                    "a",
                    Variable("a")));

            // abc 123 $$$ 456 $$$
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Variable("abc"),
                                Constant(123)),
                            Variable("$$$")),
                        Constant(456)),
                    Variable("$$$"));
            
            var actual = environment.Infer(expression);

            // $$$ (($$$ (abc 123)) 456)
            var expected =
                Apply(
                    Variable("$$$"),
                    Apply(
                        Apply(
                            Variable("$$$"),
                            Apply(
                                Variable("abc"),
                                Constant(123))),
                        Constant(456)));
            
            AssertLogicalEqual(expression, expected, actual);
        }
         
        [Test]
        public void BoundVariableWithInfixAttributes4()
        {
            var environment = CLREnvironments();
            
            // $$$ @ INFIX|LTR = a -> a
            environment.MutableBind(
                BoundVariable("$$$", InfixLeftToRight()),
                Lambda(
                    "a",
                    Variable("a")));

            // abc 123 $$$ $$$ 456
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Variable("abc"),
                                Constant(123)),
                            Variable("$$$")),
                        Variable("$$$")),
                    Constant(456));
            
            var actual = environment.Infer(expression);

            // ($$$ ($$$ (abc 123))) 456
            var expected =
                Apply(
                    Apply(
                        Variable("$$$"),
                        Apply(
                            Variable("$$$"),
                            Apply(
                                Variable("abc"),
                                Constant(123)))),
                    Constant(456));
            
            AssertLogicalEqual(expression, expected, actual);
        }
         
        [Test]
        public void BoundVariableWithInfixAttributes5()
        {
            var environment = CLREnvironments();
            
            // $$$ @ INFIX|LTR = a -> a
            environment.MutableBind(
                BoundVariable("$$$", InfixLeftToRight()),
                Lambda(
                    "a",
                    Variable("a")));

            // $$$ abc 123
            var expression =
                Apply(
                    Apply(
                        Variable("$$$"),
                        Variable("abc")),
                    Constant(123));
            
            var actual = environment.Infer(expression);

            // ($$$ abc) 123
            var expected =
                Apply(
                    Apply(
                        Variable("$$$"),
                        Variable("abc")),
                    Constant(123));
            
            AssertLogicalEqual(expression, expected, actual);
        }

        //////////////////////////////////////////////////////////
                 
        /*
        [Test]
        public void BoundVariableWithPrefixRightToLeftAttributes1()
        {
            var environment = CLREnvironments();
            
            // $$$ @ PREFIX|RTL = a -> a
            environment.MutableBind(
                BoundVariable("$$$", PrefixRightToLeft()),
                Lambda(
                    "a",
                    Variable("a")));

            // $$$ 123
            var expression =
                Apply(
                    Variable("$$$"),
                    Constant(123));
            
            var actual = environment.Infer(expression);

            // $$$ 123
            var expected =
                Apply(
                    Variable("$$$"),
                    Constant(123));
            
            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void BoundVariableWithPrefixRightToLeftAttributes2()
        {
            var environment = CLREnvironments();
            
            // $$$ @ PREFIX|RTL = a -> a
            environment.MutableBind(
                BoundVariable("$$$", PrefixRightToLeft()),
                Lambda(
                    "a",
                    Variable("a")));

            // $$$ abc 123
            var expression =
                Apply(
                    Apply(
                        Variable("$$$"),
                        Variable("abc")),
                    Constant(123));
            
            var actual = environment.Infer(expression);

            // ($$$ abc) 123
            var expected =
                Apply(
                    Apply(
                        Variable("$$$"),
                        Variable("abc")),
                    Constant(123));
            
            AssertLogicalEqual(expression, expected, actual);
        }
  
        [Test]
        public void BoundVariableWithPrefixRightToLeftAttributes3()
        {
            var environment = CLREnvironments();
            
            // $$$ @ PREFIX|RTL = a -> a
            environment.MutableBind(
                BoundVariable("$$$", PrefixRightToLeft()),
                Lambda(
                    "a",
                    Variable("a")));

            // $$$ abc def 123
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Variable("$$$"),
                            Variable("abc")),
                        Variable("def")),
                    Constant(123));
            
            var actual = environment.Infer(expression);

            // ($$$ abc) (def 456)
            var expected =
                Apply(
                    Apply(
                        Variable("$$$"),
                        Variable("abc")),
                    Apply(
                        Variable("def"),
                        Constant(123)));
            
            AssertLogicalEqual(expression, expected, actual);
        }
          
        [Test]
        public void BoundVariableWithPrefixRightToLeftAttributes4()
        {
            var environment = CLREnvironments();
            
            // $$$ @ PREFIX|RTL = a -> a
            environment.MutableBind(
                BoundVariable("$$$", PrefixRightToLeft()),
                Lambda(
                    "a",
                    Variable("a")));

            // abc $$$ 123
            var expression =
                Apply(
                    Apply(
                        Variable("abc"),
                        Variable("$$$")),
                    Constant(123));
            
            var actual = environment.Infer(expression);

            // abc $$$ 123
            var expected =
                Apply(
                    Apply(
                        Variable("abc"),
                        Variable("$$$")),
                    Constant(123));
            
            AssertLogicalEqual(expression, expected, actual);
        }
      
        [Test]
        public void BoundVariableWithPrefixRightToLeftAttributes5()
        {
            var environment = CLREnvironments();
            
            // $$$ @ PREFIX|RTL = a -> a
            environment.MutableBind(
                BoundVariable("$$$", PrefixRightToLeft()),
                Lambda(
                    "a",
                    Variable("a")));

            // abc $$$ def 123
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Variable("abc"),
                            Variable("$$$")),
                        Variable("def")),
                    Constant(123));
            
            var actual = environment.Infer(expression);

            // ((abc $$$) def) 123
            var expected =
                Apply(
                    Apply(
                        Apply(
                            Variable("abc"),
                            Variable("$$$")),
                        Variable("def")),
                    Constant(123));
            
            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void BoundVariableWithPrefixRightToLeftAttributes6()
        {
            var environment = CLREnvironments();
            
            // $$$ @ PREFIX|RTL = a -> a
            environment.MutableBind(
                BoundVariable("$$$", PrefixRightToLeft()),
                Lambda(
                    "a",
                    Variable("a")));

            // abc $$$ 123 def 456
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Variable("abc"),
                                Variable("$$$")),
                            Constant(123)),
                        Variable("def")),
                    Constant(456));
            
            var actual = environment.Infer(expression);

            // ((abc $$$) 123) (def 456)
            var expected =
                Apply(
                    Apply(
                        Apply(
                            Variable("abc"),
                            Variable("$$$")),
                        Constant(123)),
                    Apply(
                        Variable("def"),
                        Constant(456)));
            
            AssertLogicalEqual(expression, expected, actual);
        }
          
        [Test]
        public void BoundVariableWithPrefixRightToLeftAttributes7()
        {
            var environment = CLREnvironments();
            
            // $$$ @ PREFIX|RTL = a -> a
            environment.MutableBind(
                BoundVariable("$$$", PrefixRightToLeft()),
                Lambda(
                    "a",
                    Variable("a")));

            // $$$ abc $$$ def 123
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Variable("$$$"),
                                Variable("abc")),
                            Variable("$$$")),
                        Variable("def")),
                    Constant(123));
            
            var actual = environment.Infer(expression);

            // ($$$ abc) (($$$ def) 123)
            var expected =
                Apply(
                    Apply(
                        Variable("$$$"),
                        Variable("abc")),
                    Apply(
                        Apply(
                            Variable("$$$"),
                            Variable("def")),
                        Constant(123)));
            
            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void BoundVariableWithPrefixRightToLeftAttributes8()
        {
            var environment = CLREnvironments();
            
            // $$$ @ PREFIX|RTL = a -> a
            environment.MutableBind(
                BoundVariable("$$$", PrefixRightToLeft()),
                Lambda(
                    "a",
                    Variable("a")));

            // abc $$$ def $$$ ghi 123
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Apply(
                                    Variable("abc"),
                                    Variable("$$$")),
                                Variable("def")),
                            Variable("$$$")),
                        Variable("ghi")),
                    Constant(123));
            
            var actual = environment.Infer(expression);

            // ((abc $$$) def) (($$$ ghi) 123)
            var expected =
                Apply(
                    Apply(
                        Apply(
                            Variable("abc"),
                            Variable("$$$")),
                        Variable("def")),
                    Apply(
                        Apply(
                            Variable("$$$"),
                            Variable("ghi")),
                        Constant(123)));
            
            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void BoundVariableWithPrefixRightToLeftAttributes9()
        {
            var environment = CLREnvironments();
            
            // $$$ @ PREFIX|RTL = a -> a
            environment.MutableBind(
                BoundVariable("$$$", PrefixRightToLeft()),
                Lambda(
                    "a",
                    Variable("a")));

            // abc $$$ def $$$ ghi jkl 123
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Apply(
                                    Apply(
                                        Variable("abc"),
                                        Variable("$$$")),
                                    Variable("def")),
                                Variable("$$$")),
                            Variable("ghi")),
                        Variable("jkl")),
                    Constant(123));
            
            var actual = environment.Infer(expression);

            // ((abc $$$) def) (($$$ ghi) (jkl 123))
            var expected =
                Apply(
                    Apply(
                        Apply(
                            Variable("abc"),
                            Variable("$$$")),
                        Variable("def")),
                    Apply(
                        Apply(
                            Variable("$$$"),
                            Variable("ghi")),
                        Apply(
                            Variable("jkl"),
                            Constant(123))));
            
            AssertLogicalEqual(expression, expected, actual);
        }
        */

        //////////////////////////////////////////////////////////
                  
        [Test]
        public void BoundVariableWithInfixRightToLeftAttributes1()
        {
            var environment = CLREnvironments();
            
            // $$$ @ INFIX|RTL = a -> a
            environment.MutableBind(
                BoundVariable("$$$", InfixRightToLeft()),
                Lambda(
                    "a",
                    Variable("a")));

            // $$$ 123
            var expression =
                Apply(
                    Variable("$$$"),
                    Constant(123));
            
            var actual = environment.Infer(expression);

            // TODO: cause error?

            // $$$ 123
            var expected =
                Apply(
                    Variable("$$$"),
                    Constant(123));
            
            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void BoundVariableWithInfixRightToLeftAttributes2()
        {
            var environment = CLREnvironments();
            
            // $$$ @ INFIX|RTL = a -> a
            environment.MutableBind(
                BoundVariable("$$$", InfixRightToLeft()),
                Lambda(
                    "a",
                    Variable("a")));

            // $$$ abc 123
            var expression =
                Apply(
                    Apply(
                        Variable("$$$"),
                        Variable("abc")),
                    Constant(123));
            
            var actual = environment.Infer(expression);

            // TODO: cause error?

            // ($$$ abc) 123
            var expected =
                Apply(
                    Apply(
                        Variable("$$$"),
                        Variable("abc")),
                    Constant(123));
            
            AssertLogicalEqual(expression, expected, actual);
        }
  
        [Test]
        public void BoundVariableWithInfixRightToLeftAttributes3()
        {
            var environment = CLREnvironments();
            
            // $$$ @ INFIX|RTL = a -> a
            environment.MutableBind(
                BoundVariable("$$$", InfixRightToLeft()),
                Lambda(
                    "a",
                    Variable("a")));

            // $$$ abc def 123
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Variable("$$$"),
                            Variable("abc")),
                        Variable("def")),
                    Constant(123));
            
            var actual = environment.Infer(expression);

            // TODO: cause error?

            // ($$$ abc) (def 456)
            var expected =
                Apply(
                    Apply(
                        Variable("$$$"),
                        Variable("abc")),
                    Apply(
                        Variable("def"),
                        Constant(123)));
            
            AssertLogicalEqual(expression, expected, actual);
        }
          
        [Test]
        public void BoundVariableWithInfixRightToLeftAttributes4()
        {
            var environment = CLREnvironments();
            
            // $$$ @ INFIX|RTL = a -> a
            environment.MutableBind(
                BoundVariable("$$$", InfixRightToLeft()),
                Lambda(
                    "a",
                    Variable("a")));

            // abc $$$ 123
            var expression =
                Apply(
                    Apply(
                        Variable("abc"),
                        Variable("$$$")),
                    Constant(123));
            
            var actual = environment.Infer(expression);

            // ($$$ abc) 123
            var expected =
                Apply(
                    Apply(
                        Variable("$$$"),
                        Variable("abc")),
                    Constant(123));
            
            AssertLogicalEqual(expression, expected, actual);
        }
      
        [Test]
        public void BoundVariableWithInfixRightToLeftAttributes5()
        {
            var environment = CLREnvironments();
            
            // $$$ @ INFIX|RTL = a -> a
            environment.MutableBind(
                BoundVariable("$$$", InfixRightToLeft()),
                Lambda(
                    "a",
                    Variable("a")));

            // abc $$$ def 123
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Variable("abc"),
                            Variable("$$$")),
                        Variable("def")),
                    Constant(123));
            
            var actual = environment.Infer(expression);

            // ($$$ abc) (def 123)
            var expected =
                Apply(
                    Apply(
                        Variable("$$$"),
                        Variable("abc")),
                    Apply(
                        Variable("def"),
                        Constant(123)));
        
            AssertLogicalEqual(expression, expected, actual);
        }
        
        [Test]
        public void BoundVariableWithInfixRightToLeftAttributes6()
        {
            var environment = CLREnvironments();
            
            // $$$ @ INFIX|RTL = a -> a
            environment.MutableBind(
                BoundVariable("$$$", InfixRightToLeft()),
                Lambda(
                    "a",
                    Variable("a")));

            // abc $$$ def 123 456
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Variable("abc"),
                                Variable("$$$")),
                            Variable("def")),
                        Constant(123)),
                    Constant(456));
            
            var actual = environment.Infer(expression);

            // ($$$ abc) ((def 123) 456)
            var expected =
                Apply(
                    Apply(
                        Variable("$$$"),
                        Variable("abc")),
                    Apply(
                        Apply(
                            Variable("def"),
                            Constant(123)),
                        Constant(456)));
            
            AssertLogicalEqual(expression, expected, actual);
        }
          
        [Test]
        public void BoundVariableWithInfixRightToLeftAttributes7()
        {
            var environment = CLREnvironments();
            
            // $$$ @ INFIX|RTL = a -> a
            environment.MutableBind(
                BoundVariable("$$$", InfixRightToLeft()),
                Lambda(
                    "a",
                    Variable("a")));

            // $$$ abc $$$ def 123
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Variable("$$$"),
                                Variable("abc")),
                            Variable("$$$")),
                        Variable("def")),
                    Constant(123));
            
            var actual = environment.Infer(expression);

            // TODO: cause error?
            
            // ($$$ abc) (($$$ def) 123)
            var expected =
                Apply(
                    Apply(
                        Variable("$$$"),
                        Variable("abc")),
                    Apply(
                        Apply(
                            Variable("$$$"),
                            Variable("def")),
                        Constant(123)));
            
            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void BoundVariableWithInfixRightToLeftAttributes8()
        {
            var environment = CLREnvironments();
            
            // $$$ @ INFIX|RTL = a -> a
            environment.MutableBind(
                BoundVariable("$$$", InfixRightToLeft()),
                Lambda(
                    "a",
                    Variable("a")));

            // abc $$$ def $$$ ghi 123
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Apply(
                                    Variable("abc"),
                                    Variable("$$$")),
                                Variable("def")),
                            Variable("$$$")),
                        Variable("ghi")),
                    Constant(123));
            
            var actual = environment.Infer(expression);

            // ($$$ abc) (($$$ def) (ghi 123))
            var expected =
                Apply(
                    Apply(
                        Variable("$$$"),
                        Variable("abc")),
                    Apply(
                        Apply(
                            Variable("$$$"),
                            Variable("def")),
                        Apply(
                            Variable("ghi"),
                            Constant(123))));
            
            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void BoundVariableWithInfixRightToLeftAttributes9()
        {
            var environment = CLREnvironments();
            
            // $$$ @ INFIX|RTL = a -> a
            environment.MutableBind(
                BoundVariable("$$$", InfixRightToLeft()),
                Lambda(
                    "a",
                    Variable("a")));

            // abc $$$ def $$$ ghi jkl 123
            var expression =
                Apply(
                    Apply(
                        Apply(
                            Apply(
                                Apply(
                                    Apply(
                                        Variable("abc"),
                                        Variable("$$$")),
                                    Variable("def")),
                                Variable("$$$")),
                            Variable("ghi")),
                        Variable("jkl")),
                    Constant(123));
            
            var actual = environment.Infer(expression);

            // ($$$ abc) (($$$ def) ((ghi jkl) 123))
            var expected =
                Apply(
                    Apply(
                        Variable("$$$"),
                        Variable("abc")),
                    Apply(
                        Apply(
                            Variable("$$$"),
                            Variable("def")),
                        Apply(
                            Apply(
                                Variable("ghi"),
                                Variable("jkl")),
                            Constant(123))));
            
            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion
 
        ////////////////////////////////////////////////////////////////////////////////////
        
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
