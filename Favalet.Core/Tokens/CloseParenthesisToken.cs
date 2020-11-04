namespace Favalet.Tokens
{
    public sealed class CloseParenthesisToken :
        ParenthesisToken
    {
        internal CloseParenthesisToken(ParenthesisPair parenthesis) :
            base(parenthesis)
        { }

        public override char Symbol =>
            this.Pair.Close;
    }
}
