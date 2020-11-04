using Favalet.Tokens;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Favalet.Reactive.Linq;

namespace Favalet
{
    [TestFixture]
    public sealed class LexerTest
    {
        private static readonly Lexer lexer = Lexer.Create();
        private static readonly Func<string, Token[]>[] LexerRunners =
            new[]
            {
                new Func<string, Token[]>(text => lexer.Analyze(text).ToEnumerable().ToArray()),
                new Func<string, Token[]>(text => lexer.Analyze(text.AsEnumerable()).ToEnumerable().ToArray()),
                new Func<string, Token[]>(text => lexer.Analyze(new StringReader(text)).ToEnumerable().ToArray()),
            };

        ////////////////////////////////////////////////////

        [TestCaseSource("LexerRunners")]
        public void EnumerableIdentityTokens(Func<string, Token[]> run)
        {
            var text = "abc def ghi";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Identity("abc"),
                    Token.WhiteSpace(),
                    Token.Identity("def"),
                    Token.WhiteSpace(),
                    Token.Identity("ghi") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableIdentityTokensBeforeSpace(Func<string, Token[]> run)
        {
            var text = "  abc def ghi";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Identity("abc"),
                    Token.WhiteSpace(),
                    Token.Identity("def"),
                    Token.WhiteSpace(),
                    Token.Identity("ghi") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableIdentityTokensAfterSpace(Func<string, Token[]> run)
        {
            var text = "abc def ghi  ";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Identity("abc"),
                    Token.WhiteSpace(),
                    Token.Identity("def"),
                    Token.WhiteSpace(),
                    Token.Identity("ghi"),
                    Token.WhiteSpace() },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableIdentityTokensLongSpace(Func<string, Token[]> run)
        {
            var text = "abc      def      ghi";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Identity("abc"),
                    Token.WhiteSpace(),
                    Token.Identity("def"),
                    Token.WhiteSpace(),
                    Token.Identity("ghi") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableIdentityTokensBeforeBrackets(Func<string, Token[]> run)
        {
            var text = "(abc def) ghi";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Open('('),
                    Token.Identity("abc"),
                    Token.WhiteSpace(),
                    Token.Identity("def"),
                    Token.Close(')'), 
                    Token.WhiteSpace(),
                    Token.Identity("ghi") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableIdentityTokensAfterBrackets(Func<string, Token[]> run)
        {
            var text = "abc (def ghi)";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Identity("abc"),
                    Token.WhiteSpace(),
                    Token.Open('('),
                    Token.Identity("def"),
                    Token.WhiteSpace(),
                    Token.Identity("ghi"),
                    Token.Close(')') },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableIdentityTokensWithSpacingBrackets(Func<string, Token[]> run)
        {
            var text = "abc ( def ) ghi";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Identity("abc"),
                    Token.WhiteSpace(),
                    Token.Open('('),
                    Token.WhiteSpace(),
                    Token.Identity("def"),
                    Token.WhiteSpace(),
                    Token.Close(')'),
                    Token.WhiteSpace(),
                    Token.Identity("ghi") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableIdentityTokensWithNoSpacingBrackets(Func<string, Token[]> run)
        {
            var text = "abc(def)ghi";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Identity("abc"),
                    Token.Open('('), 
                    Token.Identity("def"),
                    Token.Close(')'),
                    Token.Identity("ghi") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableIdentityTrailsNumericTokens(Func<string, Token[]> run)
        {
            var text = "a12 d34 g56";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Identity("a12"),
                    Token.WhiteSpace(),
                    Token.Identity("d34"),
                    Token.WhiteSpace(),
                    Token.Identity("g56") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableSignLikeOperatorAndIdentityTokens1(Func<string, Token[]> run)
        {
            var text = "+abc";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Identity("+"),
                    Token.Identity("abc") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableSignLikeOperatorAndIdentityTokens2(Func<string, Token[]> run)
        {
            var text = "-abc";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Identity("-"),
                    Token.Identity("abc") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableStrictOperatorAndIdentityTokens1(Func<string, Token[]> run)
        {
            var text = "++abc";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Identity("++"),
                    Token.Identity("abc") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableStrictOperatorAndIdentityTokens2(Func<string, Token[]> run)
        {
            var text = "--abc";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Identity("--"),
                    Token.Identity("abc") },
                actual);
        }

        ///////////////////////////////////////////////

        [TestCaseSource("LexerRunners")]
        public void EnumerableNumericTokens(Func<string, Token[]> run)
        {
            var text = "123 456 789";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Numeric("123"),
                    Token.WhiteSpace(),
                    Token.Numeric("456"),
                    Token.WhiteSpace(),
                    Token.Numeric("789") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableCombinedIdentityAndNumericTokens(Func<string, Token[]> run)
        {
            var text = "abc 456 def";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Identity("abc"),
                    Token.WhiteSpace(),
                    Token.Numeric("456"),
                    Token.WhiteSpace(),
                    Token.Identity("def") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableNumericTokensBeforeBrackets(Func<string, Token[]> run)
        {
            var text = "(123 456) 789";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Open('('),
                    Token.Numeric("123"),
                    Token.WhiteSpace(),
                    Token.Numeric("456"),
                    Token.Close(')'),
                    Token.WhiteSpace(),
                    Token.Numeric("789") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableNumericTokensAfterBrackets(Func<string, Token[]> run)
        {
            var text = "123 (456 789)";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Numeric("123"),
                    Token.WhiteSpace(),
                    Token.Open('('),
                    Token.Numeric("456"),
                    Token.WhiteSpace(),
                    Token.Numeric("789"),
                    Token.Close(')') },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableNumericTokensWithSpacingBrackets(Func<string, Token[]> run)
        {
            var text = "123 ( 456 ) 789";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Numeric("123"),
                    Token.WhiteSpace(),
                    Token.Open('('),
                    Token.WhiteSpace(),
                    Token.Numeric("456"),
                    Token.WhiteSpace(),
                    Token.Close(')'),
                    Token.WhiteSpace(),
                    Token.Numeric("789") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableNumericTokensWithNoSpacingBrackets(Func<string, Token[]> run)
        {
            var text = "123(456)789";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Numeric("123"),
                    Token.Open('('),
                    Token.Numeric("456"),
                    Token.Close(')'),
                    Token.Numeric("789") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerablePlusSignNumericTokens(Func<string, Token[]> run)
        {
            var text = "+123 +456 +789";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.PlusSign(),
                    Token.Numeric("123"),
                    Token.WhiteSpace(),
                    Token.PlusSign(), 
                    Token.Numeric("456"),
                    Token.WhiteSpace(),
                    Token.PlusSign(),
                    Token.Numeric("789") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableMinusSignNumericTokens(Func<string, Token[]> run)
        {
            var text = "-123 -456 -789";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.MinusSign(),
                    Token.Numeric("123"),
                    Token.WhiteSpace(),
                    Token.MinusSign(),
                    Token.Numeric("456"),
                    Token.WhiteSpace(),
                    Token.MinusSign(),
                    Token.Numeric("789") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerablePlusOperatorNumericTokens(Func<string, Token[]> run)
        {
            var text = "+ 123 + 456 + 789";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Identity("+"),
                    Token.WhiteSpace(),
                    Token.Numeric("123"), 
                    Token.WhiteSpace(),
                    Token.Identity("+"),
                    Token.WhiteSpace(),
                    Token.Numeric("456"),
                    Token.WhiteSpace(),
                    Token.Identity("+"),
                    Token.WhiteSpace(),
                    Token.Numeric("789") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableMinusOperatorNumericTokens(Func<string, Token[]> run)
        {
            var text = "- 123 - 456 - 789";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Identity("-"),
                    Token.WhiteSpace(),
                    Token.Numeric("123"),
                    Token.WhiteSpace(),
                    Token.Identity("-"),
                    Token.WhiteSpace(),
                    Token.Numeric("456"),
                    Token.WhiteSpace(),
                    Token.Identity("-"),
                    Token.WhiteSpace(),
                    Token.Numeric("789") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerablePlusOperatorWithSpaceAndNumericTokens(Func<string, Token[]> run)
        {
            var text = "123 + 456";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Numeric("123"),
                    Token.WhiteSpace(),
                    Token.Identity("+"),
                    Token.WhiteSpace(),
                    Token.Numeric("456") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableMinusOperatorWithSpaceAndNumericTokens(Func<string, Token[]> run)
        {
            var text = "123 - 456";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Numeric("123"),
                    Token.WhiteSpace(),
                    Token.Identity("-"),
                    Token.WhiteSpace(),
                    Token.Numeric("456") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerablePlusOperatorSideBySideAndNumericTokens2(Func<string, Token[]> run)
        {
            var text = "123+456";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Numeric("123"),
                    Token.PlusSign(),
                    Token.Numeric("456") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableMinusOperatorSideBySideAndNumericTokens(Func<string, Token[]> run)
        {
            var text = "123-456";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Numeric("123"),
                    Token.MinusSign(),
                    Token.Numeric("456") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableComplexNumericOperatorTokens1(Func<string, Token[]> run)
        {
            var text = "-123*(+456+789)";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.MinusSign(),
                    Token.Numeric("123"),
                    Token.Identity("*"),
                    Token.Open('('),
                    Token.PlusSign(),
                    Token.Numeric("456"),
                    Token.PlusSign(),
                    Token.Numeric("789"),
                    Token.Close(')')
                },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableComplexNumericOperatorTokens2(Func<string, Token[]> run)
        {
            var text = "+123*(-456-789)";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.PlusSign(),
                    Token.Numeric("123"),
                    Token.Identity("*"),
                    Token.Open('('),
                    Token.MinusSign(),
                    Token.Numeric("456"),
                    Token.MinusSign(),
                    Token.Numeric("789"),
                    Token.Close(')')
                },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableStrictOperatorAndNumericTokens1(Func<string, Token[]> run)
        {
            var text = "++123";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Identity("++"),
                    Token.Numeric("123") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableStrictOperatorAndNumericTokens2(Func<string, Token[]> run)
        {
            var text = "--123";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Identity("--"),
                    Token.Numeric("123") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableCombineIdentityAndNumericTokensWithOperator(Func<string, Token[]> run)
        {
            var text = "abc+123";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Identity("abc"),
                    Token.PlusSign(),
                    Token.Numeric("123") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void EnumerableCombineNumericAndIdentityTokensWithOperator(Func<string, Token[]> run)
        {
            var text = "123+abc";
            var actual = run(text);

            Assert.AreEqual(
                new Token[] {
                    Token.Numeric("123"),
                    Token.Identity("+"),
                    Token.Identity("abc") },
                actual);
        }

        [TestCaseSource("LexerRunners")]
        public void Operator1Detection(Func<string, Token[]> run)
        {
            foreach (var ch in Token.OperatorChars)
            {
                var text = $"123 {ch} abc";
                var actual = run(text);

                Assert.AreEqual(
                    new Token[] {
                        Token.Numeric("123"),
                        Token.WhiteSpace(),
                        Token.Identity(ch.ToString()),
                        Token.WhiteSpace(),
                        Token.Identity("abc") },
                    actual);
            }
        }

        [TestCaseSource("LexerRunners")]
        public void Operator2Detection(Func<string, Token[]> run)
        {
            Parallel.ForEach(
                Token.OperatorChars.
                    SelectMany(ch1 => Token.OperatorChars.
                        Select(ch2 => (ch1, ch2))),
                entry =>
                {
                    var text = $"123 {entry.ch1}{entry.ch2} abc";
                    var actual = run(text);

                    Assert.AreEqual(
                        new Token[] {
                            Token.Numeric("123"),
                            Token.WhiteSpace(),
                            Token.Identity($"{entry.ch1}{entry.ch2}"),
                            Token.WhiteSpace(),
                            Token.Identity("abc") },
                        actual);
                });
        }

        [TestCaseSource("LexerRunners")]
        public void Operator3Detection(Func<string, Token[]> run)
        {
            Parallel.ForEach(
                Token.OperatorChars.
                    SelectMany(ch1 => Token.OperatorChars.
                        SelectMany(ch2 => Token.OperatorChars.
                            Select(ch3 => (ch1, ch2, ch3)))),
                entry =>
                {
                    var text = $"123 {entry.ch1}{entry.ch2}{entry.ch3} abc";
                    var actual = run(text);

                    Assert.AreEqual(
                        new Token[] {
                            Token.Numeric("123"),
                            Token.WhiteSpace(),
                            Token.Identity($"{entry.ch1}{entry.ch2}{entry.ch3}"),
                            Token.WhiteSpace(),
                            Token.Identity("abc") },
                        actual);
                });
        }
    }
}
