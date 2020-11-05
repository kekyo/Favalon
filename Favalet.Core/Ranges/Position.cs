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
using System.ComponentModel;
using System.Diagnostics;

namespace Favalet.Ranges
{
    [DebuggerStepThrough]
    public struct Position : IEquatable<Position>, IComparable<Position>, IComparable
    {
        public static readonly Position Unknown = new Position(-1, -1);
        public static readonly Position Zero = new Position(0, 0);
        public static readonly Position MaxValue = new Position(int.MaxValue, int.MaxValue);

        public readonly int Line;
        public readonly int Column;

        private Position(int line, int column)
        {
            this.Line = line;
            this.Column = column;
        }

        public Position MoveColumn(int forwardChars) =>
            new Position(this.Line, this.Column + forwardChars);
        public Position MoveLine(int forwardLines, int newColumn = 0) =>
            new Position(this.Line + forwardLines, newColumn);

        public override int GetHashCode() =>
            this.Line.GetHashCode() ^ this.Column.GetHashCode();

        public bool Equals(Position other) =>
            (this.Line == other.Line) && (this.Column == other.Column);

        public override bool Equals(object obj) =>
            obj is Position position ? this.Equals(position) : false;

        public int CompareTo(Position position) =>
            this.Line.CompareTo(position.Line) switch
            {
                0 => this.Column.CompareTo(position.Column),
                int result => result
            };

        int IComparable.CompareTo(object obj) =>
            this.CompareTo((Position)obj);

        public override string ToString() =>
            $"{this.Line},{this.Column}";

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Deconstruct(out int line, out int column)
        {
            line = this.Line;
            column = this.Column;
        }

        public static Position Create(int line, int column) =>
            new Position(line, column);

#if !NET40
        public static implicit operator Position((int line, int column) position) =>
            Create(position.line, position.column);
#endif
        
        public static bool operator <(Position lhs, Position rhs) =>
            lhs.CompareTo(rhs) < 0;
        public static bool operator <=(Position lhs, Position rhs) =>
            lhs.CompareTo(rhs) <= 0;
        public static bool operator >(Position lhs, Position rhs) =>
            lhs.CompareTo(rhs) > 0;
        public static bool operator >=(Position lhs, Position rhs) =>
            lhs.CompareTo(rhs) >= 0;

        public static Position operator +(Position lhs, int value) =>
            lhs.MoveColumn(value);
        public static Position operator +(int value, Position rhs) =>
            rhs.MoveColumn(value);
    }
}
