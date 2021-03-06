﻿/////////////////////////////////////////////////////////////////////////////////////////////////
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
using System.ComponentModel;
using System.Diagnostics;

namespace Favalet.Ranges
{
    [DebuggerStepThrough]
    public readonly struct TextPosition :
        IEquatable<TextPosition>, IComparable<TextPosition>, IComparable
    {
        public static readonly TextPosition Zero = new TextPosition(0, 0);

        public readonly int Line;
        public readonly int Column;

        private TextPosition(int line, int column)
        {
            this.Line = line;
            this.Column = column;
        }

        public TextPosition MoveColumn(int forwardChars) =>
            new TextPosition(this.Line, this.Column + forwardChars);
        public TextPosition MoveLine(int forwardLines, int newColumn = 0) =>
            new TextPosition(this.Line + forwardLines, newColumn);

        public override int GetHashCode() =>
            this.Line.GetHashCode() ^ this.Column.GetHashCode();

        public bool Equals(TextPosition other) =>
            (this.Line == other.Line) && (this.Column == other.Column);

        public override bool Equals(object? obj) =>
            obj is TextPosition position && this.Equals(position);

        public int CompareTo(TextPosition position) =>
            this.Line.CompareTo(position.Line) switch
            {
                0 => this.Column.CompareTo(position.Column),
                _ => position.Line
            };

        int IComparable.CompareTo(object? obj) =>
            this.CompareTo((TextPosition)obj!);

        public override string ToString() =>
            $"{this.Line},{this.Column}";

        public void Deconstruct(out int line, out int column)
        {
            line = this.Line;
            column = this.Column;
        }

        public static TextPosition Create(int line, int column) =>
            new TextPosition(line, column);

        public static implicit operator TextPosition((int line, int column) position) =>
            Create(position.line, position.column);
        
        public static bool operator <(TextPosition lhs, TextPosition rhs) =>
            lhs.CompareTo(rhs) < 0;
        public static bool operator <=(TextPosition lhs, TextPosition rhs) =>
            lhs.CompareTo(rhs) <= 0;
        public static bool operator >(TextPosition lhs, TextPosition rhs) =>
            lhs.CompareTo(rhs) > 0;
        public static bool operator >=(TextPosition lhs, TextPosition rhs) =>
            lhs.CompareTo(rhs) >= 0;

        public static bool operator ==(TextPosition lhs, TextPosition rhs) =>
            lhs.Equals(rhs);
        public static bool operator !=(TextPosition lhs, TextPosition rhs) =>
            !lhs.Equals(rhs);

        public static TextPosition operator +(TextPosition lhs, int value) =>
            lhs.MoveColumn(value);
        public static TextPosition operator +(int value, TextPosition rhs) =>
            rhs.MoveColumn(value);
    }
}
