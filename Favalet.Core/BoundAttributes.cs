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
using Favalet.Expressions;
using Favalet.Expressions.Operators;
using Favalet.Expressions.Specialized;
using Favalet.Contexts.Unifiers;
using Favalet.Ranges;
using Favalet.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace Favalet
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
        Binder = -10000,
        Composer = -8000,
        Lambda = -5000,
        LogicalOperators = -3000,
        Comparer = -2000,
        BitOperators = -1500,
        Addition = -1000,
        Multiply = -900,
        Neutral = 0,
        Apply = 100,
        PrefixOperators = 1000
    }

    [DebuggerStepThrough]
    [DebuggerDisplay("{Strict}")]
    public readonly struct BoundAttributes
    {
        public readonly BoundPositions Position;
        public readonly BoundAssociativities Associativity;
        public readonly BoundPrecedences Precedence;

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
            (this.Position, this.Associativity) switch
            {
                (BoundPositions.Prefix, BoundAssociativities.LeftToRight) => $"PREFIX|LTR|{this.Precedence}",
                (BoundPositions.Prefix, BoundAssociativities.RightToLeft) => $"PREFIX|RTL|{this.Precedence}",
                (BoundPositions.Infix, BoundAssociativities.LeftToRight) => $"INFIX|LTR|{this.Precedence}",
                (BoundPositions.Infix, BoundAssociativities.RightToLeft) => $"INFIX|RTL|{this.Precedence}",
                _ => string.Empty
            };

        public string Readable =>
            (this.Position, this.Associativity, this.Precedence) switch
            {
                (BoundPositions.Prefix, BoundAssociativities.LeftToRight, BoundPrecedences.Neutral) => $"PL",
                (BoundPositions.Prefix, BoundAssociativities.RightToLeft, BoundPrecedences.Neutral) => $"PR",
                (BoundPositions.Infix, BoundAssociativities.LeftToRight, BoundPrecedences.Neutral) => $"IL",
                (BoundPositions.Infix, BoundAssociativities.RightToLeft, BoundPrecedences.Neutral) => $"IR",
                (BoundPositions.Prefix, BoundAssociativities.LeftToRight, _) => $"PL{(int)this.Precedence}",
                (BoundPositions.Prefix, BoundAssociativities.RightToLeft, _) => $"PR{(int)this.Precedence}",
                (BoundPositions.Infix, BoundAssociativities.LeftToRight, _) => $"IL{(int)this.Precedence}",
                (BoundPositions.Infix, BoundAssociativities.RightToLeft, _) => $"IR{(int)this.Precedence}",
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

        public static BoundAttributes Create(
            BoundPositions position,
            BoundAssociativities associativity,
            BoundPrecedences precedence) =>
            new BoundAttributes(position, associativity, precedence);
        public static BoundAttributes Create(
            BoundPositions position,
            BoundAssociativities associativity,
            int precedence) =>
            new BoundAttributes(position, associativity, (BoundPrecedences)precedence);

        public static BoundAttributes PrefixLeftToRight(BoundPrecedences precedence) =>
            new BoundAttributes(BoundPositions.Prefix, BoundAssociativities.LeftToRight, precedence);
        public static BoundAttributes PrefixRightToLeft(BoundPrecedences precedence) =>
            new BoundAttributes(BoundPositions.Prefix, BoundAssociativities.RightToLeft, precedence);
        public static BoundAttributes InfixLeftToRight(BoundPrecedences precedence) =>
            new BoundAttributes(BoundPositions.Infix, BoundAssociativities.LeftToRight, precedence);
        public static BoundAttributes InfixRightToLeft(BoundPrecedences precedence) =>
            new BoundAttributes(BoundPositions.Infix, BoundAssociativities.RightToLeft, precedence);
        
        public static BoundAttributes PrefixLeftToRight(int precedence) =>
            new BoundAttributes(BoundPositions.Prefix, BoundAssociativities.LeftToRight, (BoundPrecedences)precedence);
        public static BoundAttributes PrefixRightToLeft(int precedence) =>
            new BoundAttributes(BoundPositions.Prefix, BoundAssociativities.RightToLeft, (BoundPrecedences)precedence);
        public static BoundAttributes InfixLeftToRight(int precedence) =>
            new BoundAttributes(BoundPositions.Infix, BoundAssociativities.LeftToRight, (BoundPrecedences)precedence);
        public static BoundAttributes InfixRightToLeft(int precedence) =>
            new BoundAttributes(BoundPositions.Infix, BoundAssociativities.RightToLeft, (BoundPrecedences)precedence);
    }
}
