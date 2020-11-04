using Favalet.Contexts;
using Favalet.Expressions;
using Favalet.Expressions.Algebraic;
using Favalet.Internal;
using NUnit.Framework;

using static Favalet.Generator;

namespace Favalet.Reducing
{
    [TestFixture]
    public sealed class AlgebraicTest
    {
        private static readonly LogicalCalculator calculator =
            LogicalCalculator.Instance;

        private static void AssertLogicalEqual(
            IExpression expression,
            IExpression expected,
            IExpression actual)
        {
            if (!calculator.Equals(expected, actual))
            {
                Assert.Fail(
                    "Expression = {0}\r\nExpected   = {1}\r\nActual     = {2}",
                    expression.GetPrettyString(PrettyStringTypes.Readable),
                    expected.GetPrettyString(PrettyStringTypes.Readable),
                    actual.GetPrettyString(PrettyStringTypes.Readable));
            }
        }

        #region And
        [Test]
        public void NonReduceSingleAnd()
        {
            var environment = Environment();

            // A && B
            var expression =
                And(
                    Variable("A"),
                    Variable("B"));

            var actual = environment.Reduce(expression);

            // A && B
            var expected =
                And(
                    Variable("A"),
                    Variable("B"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReduceSingleAnd()
        {
            var environment = Environment();

            // A && B
            var expression =
                Logical(
                    And(
                        Variable("A"),
                        Variable("B")));

            var actual = environment.Reduce(expression);

            // A && B
            var expected =
                And(
                    Variable("A"),
                    Variable("B"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void NonReduceDuplicatedAnd()
        {
            var environment = Environment();

            // (A && A) && A
            var expression =
                And(
                    And(
                        Variable("A"),
                        Variable("A")),
                    Variable("A"));

            var actual = environment.Reduce(expression);

            // (A && A) && A
            var expected =
                And(
                    And(
                        Variable("A"),
                        Variable("A")),
                    Variable("A"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReduceDuplicatedAnd()
        {
            var environment = Environment();

            // (A && A) && A
            var expression =
                Logical(
                    And(
                        And(
                            Variable("A"),
                            Variable("A")),
                        Variable("A")));

            var actual = environment.Reduce(expression);

            // A
            var expected =
                Variable("A");

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReduceMultipleDuplicatedAnd()
        {
            var environment = Environment();

            // (A && A) && (A && A)
            var expression =
                Logical(
                    And(
                        And(
                            Variable("A"),
                            Variable("A")),
                        And(
                            Variable("A"),
                            Variable("A"))));

            var actual = environment.Reduce(expression);

            // A
            var expected =
                Variable("A");

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion

        #region Or
        [Test]
        public void NonReduceSingleOr()
        {
            var environment = Environment();

            // A || B
            var expression =
                Or(
                    Variable("A"),
                    Variable("B"));

            var actual = environment.Reduce(expression);

            // A || B
            var expected =
                Or(
                    Variable("A"),
                    Variable("B"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReduceSingleOr()
        {
            var environment = Environment();

            // A || B
            var expression =
                Logical(
                    Or(
                        Variable("A"),
                        Variable("B")));

            var actual = environment.Reduce(expression);

            // A || B
            var expected =
                Or(
                    Variable("A"),
                    Variable("B"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void NonReduceDuplicatedOr()
        {
            var environment = Environment();

            // (A || A) || A
            var expression =
                Or(
                    Or(
                        Variable("A"),
                        Variable("A")),
                    Variable("A"));

            var actual = environment.Reduce(expression);

            // (A || A) || A
            var expected =
                Or(
                    Or(
                        Variable("A"),
                        Variable("A")),
                    Variable("A"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReduceDuplicatedOr()
        {
            var environment = Environment();

            // (A || A) || A
            var expression =
                Logical(
                    Or(
                        Or(
                            Variable("A"),
                            Variable("A")),
                        Variable("A")));

            var actual = environment.Reduce(expression);

            // A
            var expected =
                Variable("A");

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReduceMultipleDuplicatedOr()
        {
            var environment = Environment();

            // (A || A) || (A || A)
            var expression =
                Logical(
                    Or(
                        Or(
                            Variable("A"),
                            Variable("A")),
                        Or(
                            Variable("A"),
                            Variable("A"))));

            var actual = environment.Reduce(expression);

            // A
            var expected =
                Variable("A");

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion

        #region CombinedAndOr
        [Test]
        public void ReduceDuplicatedCombinedAndOr()
        {
            var environment = Environment();

            // (A || A) && (A || A)
            var expression =
                Logical(
                    And(
                        Or(
                            Variable("A"),
                            Variable("A")),
                        Or(
                            Variable("A"),
                            Variable("A"))));

            var actual = environment.Reduce(expression);

            // A
            var expected =
                Variable("A");

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReduceDuplicatedCombinedOrAnd()
        {
            var environment = Environment();

            // (A && A) || (A && A)
            var expression =
                Logical(
                    Or(
                        And(
                            Variable("A"),
                            Variable("A")),
                        And(
                            Variable("A"),
                            Variable("A"))));

            var actual = environment.Reduce(expression);

            // A
            var expected =
                Variable("A");

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReducePartialCombinedAndOr()
        {
            var environment = Environment();

            // (A || A) && (B || B)
            var expression =
                Logical(
                    And(
                        Or(
                            Variable("A"),
                            Variable("A")),
                        Or(
                            Variable("B"),
                            Variable("B"))));

            var actual = environment.Reduce(expression);

            // A && B
            var expected =
                And(
                    Variable("A"),
                    Variable("B"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReducePartialCombinedOrAnd()
        {
            var environment = Environment();

            // (A && A) || (B && B)
            var expression =
                Logical(
                    Or(
                        And(
                            Variable("A"),
                            Variable("A")),
                        And(
                            Variable("B"),
                            Variable("B"))));

            var actual = environment.Reduce(expression);

            // A || B
            var expected =
                Or(
                    Variable("A"),
                    Variable("B"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReducePartialDifferenceAndOr()
        {
            var environment = Environment();

            // (A || B) && (A || B)
            var expression =
                Logical(
                    And(
                        Or(
                            Variable("A"),
                            Variable("B")),
                        Or(
                            Variable("A"),
                            Variable("B"))));

            var actual = environment.Reduce(expression);

            // A || B
            var expected =
                Or(
                    Variable("A"),
                    Variable("B"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReducePartialDifferenceOrAnd()
        {
            var environment = Environment();

            // (A && B) || (A && B)
            var expression =
                Logical(
                    Or(
                        And(
                            Variable("A"),
                            Variable("B")),
                        And(
                            Variable("A"),
                            Variable("B"))));

            var actual = environment.Reduce(expression);

            // A && B
            var expected =
                And(
                    Variable("A"),
                    Variable("B"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReducePartialPartiallyAndOr()
        {
            var environment = Environment();

            // Absorption

            // A && (A || B)
            var expression =
                Logical(
                    And(
                        Variable("A"),
                        Or(
                            Variable("A"),
                            Variable("B"))));

            var actual = environment.Reduce(expression);

            // A
            var expected =
                Variable("A");

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReducePartialPartiallyOrAnd()
        {
            var environment = Environment();

            // Absorption

            // A || (A && B)
            var expression =
                Logical(
                    Or(
                        Variable("A"),
                        And(
                            Variable("A"),
                            Variable("B"))));

            var actual = environment.Reduce(expression);

            // A
            var expected =
                Variable("A");

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReducePartialAndOrTensor()
        {
            var environment = Environment();

            // (A || B) && (B || A)
            var expression =
                Logical(
                    And(
                        Or(
                            Variable("A"),
                            Variable("B")),
                        Or(
                            Variable("B"),
                            Variable("A"))));

            var actual = environment.Reduce(expression);

            // A || B
            var expected =
                Or(
                    Variable("A"),
                    Variable("B"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReducePartialOrAndTensor()
        {
            var environment = Environment();

            // (A && B) || (B && A)
            var expression =
                Logical(
                    Or(
                        And(
                            Variable("A"),
                            Variable("B")),
                        And(
                            Variable("B"),
                            Variable("A"))));

            var actual = environment.Reduce(expression);

            var expected =
                And(
                    Variable("A"),
                    Variable("B"));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReducePartialAndOrMultipleTensorLogical1()
        {
            var environment = Environment();

            // (A || (B || C)) && (B || (C || A))
            var expression =
                Logical(
                    And(
                        Or(
                            Variable("A"),
                            Or(
                                Variable("B"),
                                Variable("C"))),
                        Or(
                            Variable("B"),
                            Or(
                                Variable("C"),
                                Variable("A")))));

            var actual = environment.Reduce(expression);

            // A || B || C
            var expected =
                Or(
                    Variable("A"),
                    Or(
                        Variable("B"),
                        Variable("C")));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReducePartialOrAndMultipleTensorLogical1()
        {
            var environment = Environment();

            // (A && (B && C)) || (B && (C && A))
            var expression =
                Logical(
                    Or(
                        And(
                            Variable("A"),
                            And(
                                Variable("B"),
                                Variable("C"))),
                        And(
                            Variable("B"),
                            And(
                                Variable("C"),
                                Variable("A")))));

            var actual = environment.Reduce(expression);

            // A && B && C
            var expected =
                And(
                    Variable("A"),
                    And(
                        Variable("B"),
                        Variable("C")));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReducePartialAndOrMultipleTensorLogical2()
        {
            var environment = Environment();

            // (A || (B || C)) && ((C || A) || B)
            var expression =
                Logical(
                    And(
                        Or(
                            Variable("A"),
                            Or(
                                Variable("B"),
                                Variable("C"))),
                        Or(
                            Or(
                                Variable("C"),
                                Variable("A")),
                            Variable("B"))));

            var actual = environment.Reduce(expression);

            // A || B || C
            var expected =
                Or(
                    Variable("A"),
                    Or(
                        Variable("B"),
                        Variable("C")));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReducePartialOrAndMultipleTensorLogical2()
        {
            var environment = Environment();

            // (A && (B && C)) || ((C && A) && B)
            var expression =
                Logical(
                    Or(
                        And(
                            Variable("A"),
                            And(
                                Variable("B"),
                                Variable("C"))),
                        And(
                            And(
                                Variable("C"),
                                Variable("A")),
                            Variable("B"))));

            var actual = environment.Reduce(expression);

            // A && B && C
            var expected =
                And(
                    Variable("A"),
                    And(
                        Variable("B"),
                        Variable("C")));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReducePartialAndOrMultipleTensorLogical3()
        {
            var environment = Environment();

            // ((A || B) || C) && (B || (C || A))
            var expression =
                Logical(
                    And(
                        Or(
                            Or(
                                Variable("A"),
                                Variable("B")),
                            Variable("C")),
                        Or(
                            Variable("B"),
                            Or(
                                Variable("C"),
                                Variable("A")))));

            var actual = environment.Reduce(expression);

            // A || B || C
            var expected =
                Or(
                    Variable("A"),
                    Or(
                        Variable("B"),
                        Variable("C")));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReducePartialOrAndMultipleTensorLogical3()
        {
            var environment = Environment();

            // ((A && B) && C) || (B && (C && A))
            var expression =
                Logical(
                    Or(
                        And(
                            And(
                                Variable("A"),
                                Variable("B")),
                            Variable("C")),
                        And(
                            Variable("B"),
                            And(
                                Variable("C"),
                                Variable("A")))));

            var actual = environment.Reduce(expression);

            // A && B && C
            var expected =
                And(
                    Variable("A"),
                    And(
                        Variable("B"),
                        Variable("C")));

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReducePartialAndOrComplex()
        {
            var environment = Environment();

            // Absorption

            // (A && (A || B)) || ((C && A) && B)
            var expression =
                Logical(
                    Or(
                        And(
                            Variable("A"),
                            Or(
                                Variable("A"),
                                Variable("B"))),
                        And(
                            And(
                                Variable("C"),
                                Variable("A")),
                            Variable("B"))));

            var actual = environment.Reduce(expression);

            // A
            var expected =
                Variable("A");

            AssertLogicalEqual(expression, expected, actual);
        }

        [Test]
        public void ReducePartialOrAndComplex()
        {
            var environment = Environment();

            // Absorption

            // (A || (A && B)) && ((C || A) || B)
            var expression =
                Logical(
                    And(
                        Or(
                            Variable("A"),
                            And(
                                Variable("A"),
                                Variable("B"))),
                        Or(
                            Or(
                                Variable("C"),
                                Variable("A")),
                            Variable("B"))));

            var actual = environment.Reduce(expression);

            // A
            var expected =
                Variable("A");

            AssertLogicalEqual(expression, expected, actual);
        }
        #endregion
    }
}
