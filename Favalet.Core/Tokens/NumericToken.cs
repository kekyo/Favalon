using System;

namespace Favalet.Tokens
{
    public sealed class NumericToken :
        ValueToken, IEquatable<NumericToken?>
    {
        public readonly string Value;

        internal NumericToken(string value) =>
            this.Value = value;

        public override int GetHashCode() =>
            this.Value.GetHashCode();

        public bool Equals(NumericToken? other) =>
            other?.Value.Equals(this.Value) ?? false;

        public override bool Equals(object obj) =>
            this.Equals(obj as NumericToken);

        public override string ToString() =>
            this.Value;

        public void Deconstruct(out string value) =>
            value = this.Value;
    }
}
