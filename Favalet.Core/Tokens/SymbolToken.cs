using System;

namespace Favalet.Tokens
{
    public abstract class SymbolToken :
        Token
    {
        internal SymbolToken()
        { }

        public abstract char Symbol { get; }

        public override string ToString() =>
            this.Symbol.ToString();

        public void Deconstruct(out char symbol) =>
            symbol = this.Symbol;
    }
}
