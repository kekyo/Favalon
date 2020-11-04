using Favalet.Expressions;
using Favalet.Reactive.Linq;
using Favalet.Tokens;
using NUnit.Framework;
using System.Linq;

namespace Favalet
{
    [TestFixture]
    public sealed class ParserTest
    {
        private static IExpression[] Parse(params Token[] tokens) =>
            CLRParser.Instance.Parse(tokens).ToEnumerable().ToArray();

        [Test]
        public void EnumerableIdentityToken()
        {
            var actual = Parse(
                Token.Identity("abc"));

            Assert.AreEqual(
                new[] {
                    Generator.Variable("abc") },
                actual);
        }

        [Test]
        public void EnumerableIdentityTokens()
        {
            var actual = Parse(
                Token.Identity("abc"),
                Token.Identity("def"),
                Token.Identity("ghi"));

            Assert.AreEqual(
                new[] {
                    Generator.Apply(
                        Generator.Apply(
                            Generator.Variable("abc"),
                            Generator.Variable("def")),
                        Generator.Variable("ghi")) },
                actual);
        }

        [Test]
        public void EnumerableIdentityWithBeforeBracketTokens()
        {
            // (abc def) ghi
            var actual = Parse(
                Token.Open('('),
                Token.Identity("abc"),
                Token.Identity("def"),
                Token.Close(')'),
                Token.Identity("ghi"));

            Assert.AreEqual(
                new[] {
                    Generator.Apply(
                        Generator.Apply(
                            Generator.Variable("abc"),
                            Generator.Variable("def")),
                        Generator.Variable("ghi")) },
                actual);
        }

        [Test]
        public void EnumerableIdentityWithAfterBracketTokens()
        {
            // abc (def ghi)
            var actual = Parse(
                Token.Identity("abc"),
                Token.Open('('),
                Token.Identity("def"),
                Token.Identity("ghi"),
                Token.Close(')'));

            Assert.AreEqual(
                new[] {
                    Generator.Apply(
                        Generator.Variable("abc"),
                        Generator.Apply(
                            Generator.Variable("def"),
                            Generator.Variable("ghi"))) },
                actual);
        }

        [Test]
        public void EnumerableIdentityWithAllBracketTokens()
        {
            // (abc def ghi)
            var actual = Parse(
                Token.Open('('),
                Token.Identity("abc"),
                Token.Identity("def"),
                Token.Identity("ghi"),
                Token.Close(')'));

            Assert.AreEqual(
                new[] {
                    Generator.Apply(
                        Generator.Apply(
                            Generator.Variable("abc"),
                            Generator.Variable("def")),
                        Generator.Variable("ghi")) },
                actual);
        }

        [Test]
        public void EnumerableIdentityWithBracketToken()
        {
            // abc (def) ghi
            var actual = Parse(
                Token.Identity("abc"),
                Token.Open('('),
                Token.Identity("def"),
                Token.Close(')'),
                Token.Identity("ghi"));

            Assert.AreEqual(
                new[] {
                    Generator.Apply(
                        Generator.Apply(
                            Generator.Variable("abc"),
                            Generator.Variable("def")),
                        Generator.Variable("ghi")) },
                actual);
        }

        [Test]
        public void EnumerableIdentityWithNestedBeforeBracketsTokens()
        {
            // ((abc def) ghi) jkl
            var actual = Parse(
                Token.Open('('),
                Token.Open('('),
                Token.Identity("abc"),
                Token.Identity("def"),
                Token.Close(')'),
                Token.Identity("ghi"),
                Token.Close(')'),
                Token.Identity("jkl"));

            Assert.AreEqual(
                new[] {
                    Generator.Apply(
                        Generator.Apply(
                            Generator.Apply(
                                Generator.Variable("abc"),
                                Generator.Variable("def")),
                            Generator.Variable("ghi")),
                        Generator.Variable("jkl")) },
                actual);
        }

        [Test]
        public void EnumerableIdentityWithNestedAfterBracketsTokens()
        {
            // abc (def (ghi jkl))
            var actual = Parse(
                Token.Identity("abc"),
                Token.Open('('),
                Token.Identity("def"),
                Token.Open('('),
                Token.Identity("ghi"),
                Token.Identity("jkl"),
                Token.Close(')'),
                Token.Close(')'));

            Assert.AreEqual(
                new[] {
                    Generator.Apply(
                        Generator.Variable("abc"),
                        Generator.Apply(
                            Generator.Variable("def"),
                            Generator.Apply(
                                Generator.Variable("ghi"),
                                Generator.Variable("jkl")))) },
                actual);
        }

        //////////////////////////////////////////////

        [Test]
        public void EnumerableNumericToken()
        {
            var actual = Parse(
                Token.Numeric("123"));

            Assert.AreEqual(
                new[] {
                    CLRGenerator.Constant(123) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableNumericTokenWithSign(bool plus)
        {
            // -123    // minus sign
            var actual = Parse(
                plus ? Token.PlusSign() : Token.MinusSign(),
                Token.Numeric("123"));

            Assert.AreEqual(
                new[] {
                    CLRGenerator.Constant(plus ? 123 : -123) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableNumericTokenWithOperator(bool plus)
        {
            // - 123    // unary op
            var actual = Parse(
                Token.Identity(plus ? "+" : "-"),
                Token.WhiteSpace(),
                Token.Numeric("123"));

            Assert.AreEqual(
                new[] {
                    Generator.Apply(
                        Generator.Variable(plus ? "+" : "-"),
                        CLRGenerator.Constant(123)) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableNumericTokenWithBracketedSign(bool plus)
        {
            // (-123)    // minus sign
            var actual = Parse(
                Token.Open('('),
                plus ? Token.PlusSign() : Token.MinusSign(),
                Token.Numeric("123"),
                Token.Close(')'));

            Assert.AreEqual(
                new[] {
                    CLRGenerator.Constant(plus ? 123 : -123) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableNumericTokenWithBracketedOperator(bool plus)
        {
            // (- 123)    // unary op
            var actual = Parse(
                Token.Open('('),
                Token.Identity(plus ? "+" : "-"),
                Token.WhiteSpace(),
                Token.Numeric("123"),
                Token.Close(')'));

            Assert.AreEqual(
                new[] {
                    Generator.Apply(
                        Generator.Variable(plus ? "+" : "-"),
                        CLRGenerator.Constant(123)) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableNumericTokenCloseSignAfterIdentity(bool plus)
        {
            // abc -123    // minus sign
            var actual = Parse(
                Token.Identity("abc"),
                Token.WhiteSpace(),
                plus ? Token.PlusSign() : Token.MinusSign(),
                Token.Numeric("123"));

            Assert.AreEqual(
                new IExpression[] {
                    Generator.Apply(
                        Generator.Variable("abc"),
                        CLRGenerator.Constant(plus ? 123 : -123)) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableNumericTokenCloseBracketedSignAfterIdentity(bool plus)
        {
            // abc (-123)    // minus sign
            var actual = Parse(
                Token.Identity("abc"),
                Token.WhiteSpace(),
                Token.Open('('),
                plus ? Token.PlusSign() : Token.MinusSign(),
                Token.Numeric("123"),
                Token.Close(')'));

            Assert.AreEqual(
                new IExpression[] {
                    Generator.Apply(
                        Generator.Variable("abc"),
                        CLRGenerator.Constant(plus ? 123 : -123)) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableNumericTokenCloseNonSpacedBracketedSignAfterIdentity(bool plus)
        {
            // abc(-123)    // minus sign
            var actual = Parse(
                Token.Identity("abc"),
                Token.Open('('),
                plus ? Token.PlusSign() : Token.MinusSign(),
                Token.Numeric("123"),
                Token.Close(')'));

            Assert.AreEqual(
                new IExpression[] {
                    Generator.Apply(
                        Generator.Variable("abc"),
                        CLRGenerator.Constant(plus ? 123 : -123)) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableNumericTokenCloseSignAfterBracketedIdentity(bool plus)
        {
            // (abc) -123    // minus sign
            var actual = Parse(
                Token.Open('('),
                Token.Identity("abc"),
                Token.Close(')'),
                Token.WhiteSpace(),
                plus ? Token.PlusSign() : Token.MinusSign(),
                Token.Numeric("123"));

            Assert.AreEqual(
                new IExpression[] {
                    Generator.Apply(
                        Generator.Variable("abc"),
                        CLRGenerator.Constant(plus ? 123 : -123)) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableNumericTokenCloseSignAfterNonSpacedBracketedIdentity(bool plus)
        {
            // (abc)-123    // binary op
            var actual = Parse(
                Token.Open('('),
                Token.Identity("abc"),
                Token.Close(')'),
                plus ? Token.PlusSign() : Token.MinusSign(),
                Token.Numeric("123"));

            Assert.AreEqual(
                new IExpression[] {
                    Generator.Apply(
                        Generator.Apply(
                            Generator.Variable("abc"),
                            Generator.Variable(plus ? "+" : "-")),
                        CLRGenerator.Constant(123)) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableNumericTokenWithOperatorAfterIdentity1(bool plus)
        {
            // abc-123     // binary op
            var actual = Parse(
                Token.Identity("abc"),
                plus ? Token.PlusSign() : Token.MinusSign(),
                Token.Numeric("123"));

            // abc - 123
            Assert.AreEqual(
                new IExpression[] {
                    Generator.Apply(
                        Generator.Apply(
                            Generator.Variable("abc"),
                            Generator.Variable(plus ? "+" : "-")),
                        CLRGenerator.Constant(123)) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableNumericTokenWithOperatorAfterIdentity2(bool plus)
        {
            // abc- 123    // binary op
            var actual = Parse(
                Token.Identity("abc"),
                Token.Identity(plus ? "+" : "-"),
                Token.WhiteSpace(),
                Token.Numeric("123"));

            // abc - 123
            Assert.AreEqual(
                new IExpression[] {
                    Generator.Apply(
                        Generator.Apply(
                            Generator.Variable("abc"),
                            Generator.Variable(plus ? "+" : "-")),
                        CLRGenerator.Constant(123)) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableNumericTokenWithOperatorAfterIdentity3(bool plus)
        {
            // abc - 123   // binary op
            var actual = Parse(
                Token.Identity("abc"),
                Token.WhiteSpace(),
                Token.Identity(plus ? "+" : "-"),
                Token.WhiteSpace(),
                Token.Numeric("123"));

            // abc - 123
            Assert.AreEqual(
                new IExpression[] {
                    Generator.Apply(
                        Generator.Apply(
                            Generator.Variable("abc"),
                            Generator.Variable(plus ? "+" : "-")),
                        CLRGenerator.Constant(123)) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableIdentityTokenWithSign(bool plus)
        {
            // -abc    // unary op
            var actual = Parse(
                plus ? Token.PlusSign() : Token.MinusSign(),
                Token.Identity("abc"));

            Assert.AreEqual(
                new[] {
                    Generator.Apply(
                        Generator.Variable(plus ? "+" : "-"),
                        Generator.Variable("abc")) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableIdentityTokenWithOperator(bool plus)
        {
            // - abc    // unary op
            var actual = Parse(
                Token.Identity(plus ? "+" : "-"),
                Token.WhiteSpace(),
                Token.Identity("abc"));

            Assert.AreEqual(
                new[] {
                    Generator.Apply(
                        Generator.Variable(plus ? "+" : "-"),
                        Generator.Variable("abc")) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableIdentityTokenWithBracketedSign(bool plus)
        {
            // (-abc)    // unary op
            var actual = Parse(
                Token.Open('('),
                plus ? Token.PlusSign() : Token.MinusSign(),
                Token.Identity("abc"),
                Token.Close(')'));

            Assert.AreEqual(
                new[] {
                    Generator.Apply(
                        Generator.Variable(plus ? "+" : "-"),
                        Generator.Variable("abc")) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableIdentityTokenWithBracketedOperator(bool plus)
        {
            // (- abc)    // unary op
            var actual = Parse(
                Token.Open('('),
                Token.Identity(plus ? "+" : "-"),
                Token.WhiteSpace(),
                Token.Identity("abc"),
                Token.Close(')'));

            Assert.AreEqual(
                new[] {
                    Generator.Apply(
                        Generator.Variable(plus ? "+" : "-"),
                        Generator.Variable("abc")) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableIdentityTokenCloseSignAfterIdentity(bool plus)
        {
            // abc -def    // binary op
            var actual = Parse(
                Token.Identity("abc"),
                Token.WhiteSpace(),
                plus ? Token.PlusSign() : Token.MinusSign(),
                Token.Identity("def"));

            Assert.AreEqual(
                new IExpression[] {
                    Generator.Apply(
                        Generator.Apply(
                            Generator.Variable("abc"),
                            Generator.Variable(plus ? "+" : "-")),
                        Generator.Variable("def")) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableIdentityTokenWithOperatorAfterIdentity(bool plus)
        {
            // abc-def     // binary op
            var actual = Parse(
                Token.Identity("abc"),
                plus ? Token.PlusSign() : Token.MinusSign(),
                Token.Identity("def"));

            // abc - 123
            Assert.AreEqual(
                new IExpression[] {
                    Generator.Apply(
                        Generator.Apply(
                            Generator.Variable("abc"),
                            Generator.Variable(plus ? "+" : "-")),
                        Generator.Variable("def")) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableIdentityTokenCloseBracketedSignAfterIdentity(bool plus)
        {
            // abc (-def)    // unary op
            var actual = Parse(
                Token.Identity("abc"),
                Token.WhiteSpace(),
                Token.Open('('),
                plus ? Token.PlusSign() : Token.MinusSign(),
                Token.Identity("def"),
                Token.Close(')'));

            Assert.AreEqual(
                new IExpression[] {
                    Generator.Apply(
                        Generator.Variable("abc"),
                        Generator.Apply(
                            Generator.Variable(plus ? "+" : "-"),
                            Generator.Variable("def"))) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableIdentityTokenWithBracketedOperatorAfterIdentity(bool plus)
        {
            // abc(-def)     // unary op
            var actual = Parse(
                Token.Identity("abc"),
                Token.Open('('),
                plus ? Token.PlusSign() : Token.MinusSign(),
                Token.Identity("def"),
                Token.Close(')'));

            // abc - 123
            Assert.AreEqual(
                new IExpression[] {
                    Generator.Apply(
                        Generator.Variable("abc"),
                        Generator.Apply(
                            Generator.Variable(plus ? "+" : "-"),
                            Generator.Variable("def"))) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableIdentityTokenCloseSignAfterBracketedIdentity(bool plus)
        {
            // (abc) -def    // binary op
            var actual = Parse(
                Token.Open('('),
                Token.Identity("abc"),
                Token.Close(')'),
                Token.WhiteSpace(),
                plus ? Token.PlusSign() : Token.MinusSign(),
                Token.Identity("def"));

            Assert.AreEqual(
                new IExpression[] {
                    Generator.Apply(
                        Generator.Apply(
                            Generator.Variable("abc"),
                            Generator.Variable(plus ? "+" : "-")),
                        Generator.Variable("def")) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableIdentityTokenWithOperatorAfterBracketedIdentity(bool plus)
        {
            // (abc)-def     // binary op
            var actual = Parse(
                Token.Open('('),
                Token.Identity("abc"),
                Token.Close(')'),
                plus ? Token.PlusSign() : Token.MinusSign(),
                Token.Identity("def"));

            // abc - 123
            Assert.AreEqual(
                new IExpression[] {
                    Generator.Apply(
                        Generator.Apply(
                            Generator.Variable("abc"),
                            Generator.Variable(plus ? "+" : "-")),
                        Generator.Variable("def")) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableIdentityTokenWithOperatorAfterIdentity2(bool plus)
        {
            // abc- def    // binary op
            var actual = Parse(
                Token.Identity("abc"),
                Token.Identity(plus ? "+" : "-"),
                Token.WhiteSpace(),
                Token.Identity("def"));

            // abc - def
            Assert.AreEqual(
                new IExpression[] {
                    Generator.Apply(
                        Generator.Apply(
                            Generator.Variable("abc"),
                            Generator.Variable(plus ? "+" : "-")),
                        Generator.Variable("def")) },
                actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EnumerableIdentityTokenWithOperatorAfterIdentity3(bool plus)
        {
            // abc - def   // binary op
            var actual = Parse(
                Token.Identity("abc"),
                Token.WhiteSpace(),
                Token.Identity(plus ? "+" : "-"),
                Token.WhiteSpace(),
                Token.Identity("def"));

            // abc - 123
            Assert.AreEqual(
                new IExpression[] {
                    Generator.Apply(
                        Generator.Apply(
                            Generator.Variable("abc"),
                            Generator.Variable(plus ? "+" : "-")),
                        Generator.Variable("def")) },
                actual);
        }
    }
}
