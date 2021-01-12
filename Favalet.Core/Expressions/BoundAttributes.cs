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

using Favalet.Contexts;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Favalet.Expressions
{
    public enum BoundTypes
    {
        None,
        Prefix,
        Postfix,
        InfixLeftToRight,
        InfixRightToLeft,
    }
        
    public enum BoundPrecedences
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        Bottom = int.MinValue,
        Binder = -7000,
        Composer = -6000,
        Lambda = -5000,
        LogicalOperators = -4000,
        Comparer = -3000,
        BitOperators = -2000,
        Addition = -1000,
        Multiply = -900,
        Neutral = 0,
        PrefixOperators = 2000,
        [EditorBrowsable(EditorBrowsableState.Never)]
        Top = int.MaxValue
    }

    [DebuggerStepThrough]
    [DebuggerDisplay("{Strict}")]
    public readonly struct BoundAttributes
    {
        public readonly BoundTypes Type;
        public readonly BoundPrecedences Precedence;

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private BoundAttributes(
            BoundTypes type,
            BoundPrecedences precedence)
        {
            this.Type = type;
            this.Precedence = precedence;
        }

        public string Strict =>
            (this.Type, this.Precedence) switch
            {
                (BoundTypes.Prefix, BoundPrecedences.Neutral) => "PREFIX",
                (BoundTypes.Postfix, BoundPrecedences.Neutral) => "POSTFIX",
                (BoundTypes.InfixLeftToRight, BoundPrecedences.Neutral) => "INFIX|LTR",
                (BoundTypes.InfixRightToLeft, BoundPrecedences.Neutral) => "INFIX|RTL",
                (BoundTypes.Prefix, _) => $"PREFIX|{this.Precedence}",
                (BoundTypes.Postfix, _) => $"POSTFIX|{this.Precedence}",
                (BoundTypes.InfixLeftToRight, _) => $"INFIX|LTR|{this.Precedence}",
                (BoundTypes.InfixRightToLeft, _) => $"INFIX|RTL|{this.Precedence}",
                _ => string.Empty
            };

        public string Readable =>
            (this.Type, this.Precedence) switch
            {
                (BoundTypes.Prefix, BoundPrecedences.Neutral) => "PR",
                (BoundTypes.Postfix, BoundPrecedences.Neutral) => "PO",
                (BoundTypes.InfixLeftToRight, BoundPrecedences.Neutral) => "IL",
                (BoundTypes.InfixRightToLeft, BoundPrecedences.Neutral) => "IR",
                (BoundTypes.Prefix, _) => $"PR{this.Precedence}",
                (BoundTypes.Postfix, _) => $"PO{this.Precedence}",
                (BoundTypes.InfixLeftToRight, _) => $"IL{this.Precedence}",
                (BoundTypes.InfixRightToLeft, _) => $"IR{this.Precedence}",
                _ => string.Empty
            };
        
        public IEnumerable<XAttribute> GetXmlValues(IXmlRenderContext context) =>
            new[]
            {
                new XAttribute("type", this.Type),
                new XAttribute("precedence", this.Precedence)
            };

        public string ToString(PrettyStringTypes type) =>
            (type >= PrettyStringTypes.Strict, type >= PrettyStringTypes.Readable) switch
            {
                (true, _) => "@" + this.Strict,
                (_, true) => "@" + this.Readable,
                _ => ""
            };

        public override string ToString() =>
            this.Strict;

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Deconstruct(
            out BoundTypes type,
            out BoundPrecedences precedence)
        {
            type = this.Type;
            precedence = this.Precedence;
        }

        public static readonly BoundAttributes Neutral =
            new BoundAttributes(BoundTypes.None, BoundPrecedences.Neutral);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes Create(
            BoundTypes type,
            BoundPrecedences precedence) =>
            new BoundAttributes(type, precedence);
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes Create(
            BoundTypes type,
            int precedence) =>
            new BoundAttributes(type, (BoundPrecedences)precedence);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes None(BoundPrecedences precedence) =>
            new BoundAttributes(BoundTypes.None, precedence);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes Prefix(BoundPrecedences precedence) =>
            new BoundAttributes(BoundTypes.Prefix, precedence);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes Postfix(BoundPrecedences precedence) =>
            new BoundAttributes(BoundTypes.Postfix, precedence);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes InfixLeftToRight(BoundPrecedences precedence) =>
            new BoundAttributes(BoundTypes.InfixLeftToRight, precedence);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes InfixRightToLeft(BoundPrecedences precedence) =>
            new BoundAttributes(BoundTypes.InfixRightToLeft, precedence);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes None(int precedence) =>
            new BoundAttributes(BoundTypes.None, (BoundPrecedences)precedence);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes Prefix(int precedence) =>
            new BoundAttributes(BoundTypes.Prefix, (BoundPrecedences)precedence);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes Postfix(int precedence) =>
            new BoundAttributes(BoundTypes.Postfix, (BoundPrecedences)precedence);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes InfixLeftToRight(int precedence) =>
            new BoundAttributes(BoundTypes.InfixLeftToRight, (BoundPrecedences)precedence);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes InfixRightToLeft(int precedence) =>
            new BoundAttributes(BoundTypes.InfixRightToLeft, (BoundPrecedences)precedence);
    }
}
