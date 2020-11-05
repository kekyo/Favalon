/////////////////////////////////////////////////////////////////////////////////////////////////
//
// Favalon - An Interactive Shell Based on a Typed Lambda Calculus.
// Copyright (c) 2018-2020 Kouji Matsui (@kozy_kekyo, @kekyo2)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
/////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using Favalet.Ranges;

namespace Favalet.Tokens
{
    [DebuggerStepThrough]
    public sealed class IdentityToken :
        ValueToken, IEquatable<IdentityToken?>
    {
        public new readonly string Identity;

        private IdentityToken(string identity, TextRange range) :
            base(range) =>
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
        public void Deconstruct(out string identity, out TextRange range)
        {
            identity = this.Identity;
            range = this.Range;
        }

        public static IdentityToken Create(string identity, TextRange range) =>
            new IdentityToken(identity, range);
    }
}
