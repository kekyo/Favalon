using System;

namespace Favalet.Tokens
{
    public sealed class WhiteSpaceToken :
        Token, IEquatable<WhiteSpaceToken?>
    {
        private WhiteSpaceToken()
        { }

        public override int GetHashCode() =>
            0;

        public bool Equals(WhiteSpaceToken? other) =>
            other != null;

        public override bool Equals(object obj) =>
            this.Equals(obj as WhiteSpaceToken);

        public override string ToString() =>
            string.Empty;

        internal static readonly WhiteSpaceToken Instance =
            new WhiteSpaceToken();
    }
}
