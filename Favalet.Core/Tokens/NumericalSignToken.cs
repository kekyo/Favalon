namespace Favalet.Tokens
{
    public enum NumericalSignes
    {
        Unknown = 0,
        Plus = 1,
        Minus = -1
    }

    public sealed class NumericalSignToken :
        SymbolToken
    {
        public readonly NumericalSignes Sign;

        private NumericalSignToken(NumericalSignes sign) =>
            this.Sign = sign;

        public override char Symbol =>
            this.Sign switch
            {
                NumericalSignes.Plus => '+',
                NumericalSignes.Minus => '-',
                _ => '?'
            };

        public override int GetHashCode() =>
            this.Sign.GetHashCode();

        public bool Equals(NumericalSignToken? other) =>
            other?.Sign.Equals(this.Sign) ?? false;

        public override bool Equals(object obj) =>
            this.Equals(obj as NumericalSignToken);

        public void Deconstruct(out NumericalSignes sign) =>
            sign = this.Sign;

        internal static readonly NumericalSignToken Plus =
            new NumericalSignToken(NumericalSignes.Plus);
        internal static readonly NumericalSignToken Minus =
            new NumericalSignToken(NumericalSignes.Minus);
    }
}
