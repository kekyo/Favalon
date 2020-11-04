using System;
using System.Collections.Generic;

namespace Favalet.Tokens
{
    partial class Token
    {
        public static IEnumerable<char> OperatorChars =>
            TokenUtilities.operatorChars;

        public static IdentityToken Identity(string identity) =>
            new IdentityToken(identity);

        public static NumericalSignToken PlusSign() =>
            NumericalSignToken.Plus;
        public static NumericalSignToken MinusSign() =>
            NumericalSignToken.Minus;

        public static OpenParenthesisToken Open(char symbol) =>
            TokenUtilities.IsOpenParenthesis(symbol) is ParenthesisPair parenthesis ?
                new OpenParenthesisToken(parenthesis) :
                throw new InvalidOperationException();

        public static CloseParenthesisToken Close(char symbol) =>
            TokenUtilities.IsCloseParenthesis(symbol) is ParenthesisPair parenthesis ?
                new CloseParenthesisToken(parenthesis) :
                throw new InvalidOperationException();

        public static WhiteSpaceToken WhiteSpace() =>
            WhiteSpaceToken.Instance;

        public static NumericToken Numeric(string value) =>
            new NumericToken(value);
    }
}
