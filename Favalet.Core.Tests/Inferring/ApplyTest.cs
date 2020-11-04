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
            var environment = CLREnvironment();

            // a b
            var expression =
                Apply(
                    Variable("a"),
                    Variable("b"));

            var actual = environment.Infer(expression);

            // (a:('0 -> '1) b:'0):'1
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var ph1 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Variable("a", Function(ph0, ph1)),
                    Variable("b", ph0),
                    ph1);

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyNestedFunctionWithoutAnnotation1()
        {
            var environment = CLREnvironment();

            // a a
            var expression =
                Apply(
                    Variable("a"),
                    Variable("a"));

            var actual = environment.Infer(expression);

            // (a:('0 -> '1) a:'0):'1
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var ph1 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Variable("a", Function(ph0, ph1)),
                    Variable("a", ph0),
                    ph1);

            AssertLogicalEqual(expression, expected, actual);
        }

        //[Test]
        public void ApplyNestedFunctionWithoutAnnotation2()
        {
            var environment = CLREnvironment();

            // a = x -> x
            environment.MutableBind(
                "a",
                Lambda(
                    "x",
                    Variable("x")));

            // a a
            var expression =
                Apply(
                    Variable("a"),
                    Variable("a"));

            var actual = environment.Infer(expression);

            // ((x:('0 -> '0) -> x:('0 -> '0)) (x:'0 -> x:'0)):('0 -> '0)
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Variable(
                        "a",
                        Function(
                            Function(ph0, ph0),
                            Function(ph0, ph0))),
                    Variable(
                        "a",
                        Function(ph0, ph0)),
                    Function(
                        ph0,
                        ph0));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyWithAnnotation1()
        {
            var environment = CLREnvironment();

            // a:(bool -> _) b
            var expression =
                Apply(
                    Variable("a",
                        Function(
                            Variable("bool"),
                            Unspecified())),
                    Variable("b"));

            var actual = environment.Infer(expression);

            // (a:(bool -> '0) b:bool):'0
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Variable("a",
                        Function(Variable("bool"), ph0)),
                    Variable("b", Variable("bool")),
                    ph0);

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyWithAnnotation2()
        {
            var environment = CLREnvironment();

            // a:(_ -> bool) b
            var expression =
                Apply(
                    Variable("a",
                        Function(
                            Unspecified(),
                            Variable("bool"))),
                    Variable("b"));

            var actual = environment.Infer(expression);

            // (a:('0 -> bool) b:'0):bool
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Variable("a",
                        Function(ph0, Variable("bool"))),
                    Variable("b", ph0),
                    Variable("bool"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyWithAnnotation3()
        {
            var environment = CLREnvironment();

            // a:(int -> bool) b
            var expression =
                Apply(
                    Variable("a",
                        Function(
                            Variable("int"),
                            Variable("bool"))),
                    Variable("b"));

            var actual = environment.Infer(expression);

            // (a:(int -> bool) b:int):bool
            var expected =
                Apply(
                    Variable("a",
                        Function(
                            Variable("int"),
                            Variable("bool"))),
                    Variable("b", Variable("int")),
                    Variable("bool"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyWithAnnotation4()
        {
            var environment = CLREnvironment();

            // a b:bool
            var expression =
                Apply(
                    Variable("a"),
                    Variable("b", Variable("bool")));

            var actual = environment.Infer(expression);

            // (a:(bool -> _) b:bool):_
            var expected =
                Apply(
                    Variable("a",
                        Function(
                            Variable("bool"),
                            Unspecified())),
                    Variable("b", Variable("bool")),
                    Unspecified());

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyWithAnnotation5()
        {
            var environment = CLREnvironment();

            // (a b):bool
            var expression =
                Apply(
                    Variable("a"),
                    Variable("b"),
                    Variable("bool"));

            var actual = environment.Infer(expression);

            // (a:('0 -> bool) b:'0):bool
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Variable("a",
                        Function(
                            ph0,
                            Variable("bool"))),
                    Variable("b", ph0),
                    Variable("bool"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyWithAnnotation6()
        {
            var environment = CLREnvironment();

            // (a b:bool):int
            var expression =
                Apply(
                    Variable("a"),
                    Variable("b", Variable("bool")),
                    Variable("int"));

            var actual = environment.Infer(expression);

            // (a:(bool -> int) b:bool):int
            var expected =
                Apply(
                    Variable("a",
                        Function(
                            Variable("bool"),
                            Variable("int"))),
                    Variable("b", Variable("bool")),
                    Variable("int"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyWithAnnotation7()
        {
            var environment = CLREnvironment();

            // a:(_ -> int) b:bool
            var expression =
                Apply(
                    Variable("a",
                        Function(
                            Unspecified(),
                            Variable("int"))),
                    Variable("b", Variable("bool")));

            var actual = environment.Infer(expression);

            // (a:(bool -> int) b:bool):int
            var expected =
                Apply(
                    Variable("a",
                        Function(
                            Variable("bool"),
                            Unspecified())),
                    Variable("b", Variable("bool")),
                    Unspecified());

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyWithAnnotation8()
        {
            var environment = CLREnvironment();

            // a:(_ -> int) b:bool
            var expression =
                Apply(
                    Variable("a",
                        Function(
                            Unspecified(),
                            Variable("int"))),
                    Variable("b", Variable("bool")));

            var actual = environment.Infer(expression);

            // (a:(bool -> int) b:bool):int
            var expected =
                Apply(
                    Variable("a",
                        Function(
                            Variable("bool"),
                            Unspecified())),
                    Variable("b", Variable("bool")),
                    Unspecified());

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyWithAnnotation9()
        {
            var environment = CLREnvironment();

            // (a:(bool -> _) b):int
            var expression =
                Apply(
                    Variable("a",
                        Function(
                            Variable("bool"),
                            Unspecified())),
                    Variable("b"),
                    Variable("int"));

            var actual = environment.Infer(expression);

            // (a:(bool -> int) b:bool):int
            var expected =
                Apply(
                    Variable("a",
                        Function(
                            Variable("bool"),
                            Variable("int"))),
                    Variable("b", Variable("bool")),
                    Variable("int"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ApplyWithAnnotation10()
        {
            var environment = CLREnvironment();

            // (a:(bool -> int) b:bool):int
            var expression =
                Apply(
                    Variable("a",
                        Function(
                            Variable("bool"),
                            Variable("int"))),
                    Variable("b", Variable("bool")),
                    Variable("int"));

            var actual = environment.Infer(expression);

            // (a:(bool -> int) b:bool):int
            var expected =
                Apply(
                    Variable("a",
                        Function(
                            Variable("bool"),
                            Variable("int"))),
                    Variable("b", Variable("bool")),
                    Variable("int"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void NestedApplyWithoutAnnotation()
        {
            var environment = CLREnvironment();

            // a b c
            var expression =
                Apply(
                    Apply(
                        Variable("a"),
                        Variable("b")),
                    Variable("c"));

            var actual = environment.Infer(expression);

            // ((a:('0 -> ('1 -> '2)) b:'0 c:'1):'2
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var ph1 = provider.CreatePlaceholder();
            var ph2 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Apply(
                        Variable("a",
                            Function(
                                ph0,
                                Function(ph1, ph2))),
                        Variable("b", ph0)),
                    Variable("c", ph1),
                    ph2);

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void NestedApplyWithAnnotation1()
        {
            var environment = CLREnvironment();

            // a b c:bool
            var expression =
                Apply(
                    Apply(
                        Variable("a"),
                        Variable("b")),
                    Variable("c", Variable("bool")));

            var actual = environment.Infer(expression);

            // ((a:('0 -> (bool -> '1)) b:'0 c:bool):'1
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var ph1 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Apply(
                        Variable("a",
                            Function(
                                ph0,
                                Function(Variable("bool"), ph1))),
                        Variable("b", ph0)),
                    Variable("c", Variable("bool")),
                    ph1);

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void NestedApplyWithAnnotation2()
        {
            var environment = CLREnvironment();

            // a b:bool c
            var expression =
                Apply(
                    Apply(
                        Variable("a"),
                        Variable("b", Variable("bool"))),
                    Variable("c"));

            var actual = environment.Infer(expression);

            // ((a:(bool -> ('0 -> '1)) b:bool c:'0):'1
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var ph1 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Apply(
                        Variable("a",
                            Function(
                                Variable("bool"),
                                Function(ph0, ph1))),
                        Variable("b", Variable("bool"))),
                    Variable("c", ph0),
                    ph1);

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void NestedApplyWithAnnotation3()
        {
            var environment = CLREnvironment();

            // a:(bool -> _) b c
            var expression =
                Apply(
                    Apply(
                        Variable("a",
                            Function(Variable("bool"), Unspecified())),
                        Variable("b")),
                    Variable("c"));

            var actual = environment.Infer(expression);

            // ((a:(bool -> ('0 -> '1)) b:bool c:'0):'1
            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var ph1 = provider.CreatePlaceholder();
            var expected =
                Apply(
                    Apply(
                        Variable("a",
                            Function(
                                Variable("bool"),
                                Function(ph0, ph1))),
                        Variable("b", Variable("bool"))),
                    Variable("c", ph0),
                    ph1);

            AssertLogicalEqual(expression, expected, actual);
        }

        //[Test]
        public void ApplyYCombinator1()
        {
            var environment = CLREnvironment();

            // Y = f -> f (Y f)
            environment.MutableBind(
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

            var actual = environment.Infer(expression);

            var expected =
                Variable("TODO:");

            AssertLogicalEqual(expression, expected, actual);
        }

        //[Test]
        public void ApplyYCombinator2()
        {
            var environment = CLREnvironment();

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

            var actual = environment.Infer(expression);

            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Variable("TODO:");

            AssertLogicalEqual(expression, expected, actual);
        }

        //[Test]
        public void ApplyZCombinator()
        {
            var environment = CLREnvironment();

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

            var actual = environment.Infer(expression);

            var provider = PseudoPlaceholderProvider.Create();
            var ph0 = provider.CreatePlaceholder();
            var expected =
                Variable("TODO:");

            AssertLogicalEqual(expression, expected, actual);
        }


        // TODO: 異なる型でInferする場合をテストする
        //    1: 代入互換性が無いものはその場でエラー？ 又はASTを変形しないで返すとか？
        //    2: 代入互換性があるもの
        //    2-1: 正方向？（例えば引数）
        //    2-2: 逆方向？（例えば戻り値）


        // TODO: Unmatched orders
    }
}
