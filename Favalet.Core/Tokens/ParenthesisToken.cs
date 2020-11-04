using System;

namespace Favalet.Tokens
{
    public struct ParenthesisPair
    {
        public readonly char Open;
        public readonly char Close;

        internal ParenthesisPair(char open, char close)
        {
            this.Open = open;
            this.Close = close;
        }

        public override string ToString() =>
            $"'{this.Open}','{this.Close}'";
    }

    public abstract class ParenthesisToken :
        SymbolToken, IEquatable<ParenthesisToken?>
    {
        public readonly ParenthesisPair Pair;

        internal ParenthesisToken(ParenthesisPair parenthesis) =>
            this.Pair = parenthesis;

        public override int GetHashCode() =>
            this.Pair.GetHashCode();

        public bool Equals(ParenthesisToken? other) =>
            other?.Pair.Equals(this.Pair) ?? false;

        public override bool Equals(object obj) =>
            this.Equals(obj as ParenthesisToken);

        public void Deconstruct(out ParenthesisPair parenthesis) =>
            parenthesis = this.Pair;
    }
}
