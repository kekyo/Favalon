namespace Favalet.Tokens
{
    public sealed class OpenParenthesisToken :
        ParenthesisToken
    {
        internal OpenParenthesisToken(ParenthesisPair parenthesis) :
            base(parenthesis)
        { }

        public override char Symbol =>
            this.Pair.Open;
    }
}
