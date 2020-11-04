using System;

namespace Favalet.Tokens
{
    public sealed class IdentityToken :
        ValueToken, IEquatable<IdentityToken?>
    {
        public new readonly string Identity;

        internal IdentityToken(string identity) =>
            this.Identity = identity;

        public override int GetHashCode() =>
            this.Identity.GetHashCode();

        public bool Equals(IdentityToken? other) =>
            other?.Identity.Equals(this.Identity) ?? false;

        public override bool Equals(object obj) =>
            this.Equals(obj as IdentityToken);

        public override string ToString() =>
            this.Identity;

        public void Deconstruct(out string identity) =>
            identity = this.Identity;
    }
}
