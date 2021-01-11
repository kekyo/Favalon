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
    public enum BoundPositions
    {
        Prefix,
        Infix
    }

    public enum BoundAssociativities
    {
        LeftToRight,
        RightToLeft 
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
        public readonly BoundPositions Position;
        public readonly BoundAssociativities Associativity;
        public readonly BoundPrecedences Precedence;

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private BoundAttributes(
            BoundPositions position,
            BoundAssociativities associativity,
            BoundPrecedences precedence)
        {
            this.Position = position;
            this.Associativity = associativity;
            this.Precedence = precedence;
        }

        public string Strict =>
            (this.Position, this.Associativity, this.Precedence) switch
            {
                (BoundPositions.Prefix, BoundAssociativities.LeftToRight, BoundPrecedences.Neutral) => $"PREFIX|LTR",
                (BoundPositions.Prefix, BoundAssociativities.RightToLeft, BoundPrecedences.Neutral) => $"PREFIX|RTL",
                (BoundPositions.Infix, BoundAssociativities.LeftToRight, BoundPrecedences.Neutral) => $"INFIX|LTR",
                (BoundPositions.Infix, BoundAssociativities.RightToLeft, BoundPrecedences.Neutral) => $"INFIX|RTL",
                (BoundPositions.Prefix, BoundAssociativities.LeftToRight, _) => $"PREFIX|LTR|{this.Precedence}",
                (BoundPositions.Prefix, BoundAssociativities.RightToLeft, _) => $"PREFIX|RTL|{this.Precedence}",
                (BoundPositions.Infix, BoundAssociativities.LeftToRight, _) => $"INFIX|LTR|{this.Precedence}",
                (BoundPositions.Infix, BoundAssociativities.RightToLeft, _) => $"INFIX|RTL|{this.Precedence}",
                _ => string.Empty
            };

        public string Readable =>
            (this.Position, this.Associativity, this.Precedence) switch
            {
                (BoundPositions.Prefix, BoundAssociativities.LeftToRight, BoundPrecedences.Neutral) => $"PL",
                (BoundPositions.Prefix, BoundAssociativities.RightToLeft, BoundPrecedences.Neutral) => $"PR",
                (BoundPositions.Infix, BoundAssociativities.LeftToRight, BoundPrecedences.Neutral) => $"IL",
                (BoundPositions.Infix, BoundAssociativities.RightToLeft, BoundPrecedences.Neutral) => $"IR",
                (BoundPositions.Prefix, BoundAssociativities.LeftToRight, _) => $"PL{this.Precedence}",
                (BoundPositions.Prefix, BoundAssociativities.RightToLeft, _) => $"PR{this.Precedence}",
                (BoundPositions.Infix, BoundAssociativities.LeftToRight, _) => $"IL{this.Precedence}",
                (BoundPositions.Infix, BoundAssociativities.RightToLeft, _) => $"IR{this.Precedence}",
                _ => string.Empty
            };
        
        public IEnumerable<XAttribute> GetXmlValues(IXmlRenderContext context) =>
            new[]
            {
                new XAttribute("position", this.Position),
                new XAttribute("associativity", this.Associativity),
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
            out BoundPositions position,
            out BoundAssociativities associativity,
            out BoundPrecedences precedence)
        {
            position = this.Position;
            associativity = this.Associativity;
            precedence = this.Precedence;
        }

        public static readonly BoundAttributes Neutral =
            new BoundAttributes(BoundPositions.Prefix, BoundAssociativities.LeftToRight, BoundPrecedences.Neutral);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes Create(
            BoundPositions position,
            BoundAssociativities associativity,
            BoundPrecedences precedence) =>
            new BoundAttributes(position, associativity, precedence);
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes Create(
            BoundPositions position,
            BoundAssociativities associativity,
            int precedence) =>
            new BoundAttributes(position, associativity, (BoundPrecedences)precedence);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes PrefixLeftToRight(BoundPrecedences precedence) =>
            new BoundAttributes(BoundPositions.Prefix, BoundAssociativities.LeftToRight, precedence);
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes PrefixRightToLeft(BoundPrecedences precedence) =>
            new BoundAttributes(BoundPositions.Prefix, BoundAssociativities.RightToLeft, precedence);
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes InfixLeftToRight(BoundPrecedences precedence) =>
            new BoundAttributes(BoundPositions.Infix, BoundAssociativities.LeftToRight, precedence);
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes InfixRightToLeft(BoundPrecedences precedence) =>
            new BoundAttributes(BoundPositions.Infix, BoundAssociativities.RightToLeft, precedence);
        
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes PrefixLeftToRight(int precedence) =>
            new BoundAttributes(BoundPositions.Prefix, BoundAssociativities.LeftToRight, (BoundPrecedences)precedence);
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes PrefixRightToLeft(int precedence) =>
            new BoundAttributes(BoundPositions.Prefix, BoundAssociativities.RightToLeft, (BoundPrecedences)precedence);
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes InfixLeftToRight(int precedence) =>
            new BoundAttributes(BoundPositions.Infix, BoundAssociativities.LeftToRight, (BoundPrecedences)precedence);
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static BoundAttributes InfixRightToLeft(int precedence) =>
            new BoundAttributes(BoundPositions.Infix, BoundAssociativities.RightToLeft, (BoundPrecedences)precedence);
    }
}
