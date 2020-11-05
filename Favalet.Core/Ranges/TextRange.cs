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
    public readonly struct TextRange
    {
        public static readonly Uri UnknownText =
            new Uri("unknown.fv", UriKind.RelativeOrAbsolute);
        public static readonly TextRange Unknown =
            new TextRange(UnknownText, TextPosition.Unknown, TextPosition.Unknown);

        public readonly Uri Text;
        public readonly TextPosition First;
        public readonly TextPosition Last;

        private TextRange(Uri text, TextPosition first, TextPosition last)
        {
            this.Text = text;
            this.First = first;
            this.Last = last;
        }

        public bool Contains(TextRange inside) =>
            this.Text.Equals(inside.Text) &&
            ((this.First.Line < inside.First.Line) || ((this.First.Line == inside.First.Line) && (this.First.Column <= inside.First.Column))) &&
            ((inside.Last.Line < this.Last.Line) || ((inside.Last.Line == this.Last.Line) && (inside.Last.Column <= this.Last.Column)));

        public bool Overlaps(TextRange rhs) =>
            this.Text.Equals(rhs.Text) &&
            (this.First <= rhs.Last) && (this.Last >= rhs.First);

        public TextRange Combine(TextRange range) =>
            this.Text.Equals(range.Text) ?
                this.Combine(range.First, range.Last) :
                throw new InvalidOperationException();
        public TextRange Combine(TextPosition first, TextPosition last) =>
            new TextRange(
                this.Text,
                (this.First < first) ? this.First : first,
                (this.Last < last) ? this.Last : last);

        public TextRange Subtract(TextRange range) =>
            this.Text.Equals(range.Text) ?
                this.Subtract(range.First, range.Last) :
                throw new InvalidOperationException();
        public TextRange Subtract(TextPosition first, TextPosition last) =>
            new TextRange(
                this.Text,
                this.Contains(Create(this.Text, first)) ? first : this.First,
                this.Contains(Create(this.Text, last)) ? last : this.Last);

        public string NormalizedText =>
            this.Text.Equals(UnknownText) ? string.Empty : this.Text.ToString();
        
        public override string ToString() =>
            (this.First.Equals(this.Last)) ?
                $"{this.NormalizedText}({this.First})" :
                $"{this.NormalizedText}({this.First},{this.Last})";

        public void Deconstruct(out Uri text, out TextPosition first, out TextPosition last)
        {
            text = this.Text;
            first = this.First;
            last = this.Last;
        }
        public void Deconstruct(out Uri text, out int lineFirst, out int columnFirst, out int lineLast, out int columnLast)
        {
            text = this.Text;
            lineFirst = this.First.Line;
            columnFirst = this.First.Column;
            lineLast = this.Last.Line;
            columnLast = this.Last.Column;
        }

        public static TextRange Create(Uri text, TextPosition position) =>
            new TextRange(text, position, position);
        public static TextRange Create(Uri text, TextPosition first, TextPosition last) =>
            new TextRange(text, first, last);

#if !NET40
        public static implicit operator TextRange((int line, int column) position) =>
            Create(UnknownText, TextPosition.Create(position.line, position.column));
        public static implicit operator TextRange((int lineFirst, int columnFirst, int lineLast, int columnLast) range) =>
            Create(UnknownText, TextPosition.Create(range.lineFirst, range.columnFirst), TextPosition.Create(range.lineLast, range.columnLast));
        public static implicit operator TextRange((Uri text, int line, int column) position) =>
            Create(position.text, TextPosition.Create(position.line, position.column));
        public static implicit operator TextRange((Uri text, int lineFirst, int columnFirst, int lineLast, int columnLast) range) =>
            Create(range.text, TextPosition.Create(range.lineFirst, range.columnFirst), TextPosition.Create(range.lineLast, range.columnLast));
        public static implicit operator TextRange((string text, int line, int column) position) =>
            Create(new Uri(position.text, UriKind.RelativeOrAbsolute), TextPosition.Create(position.line, position.column));
        public static implicit operator TextRange((string text, int lineFirst, int columnFirst, int lineLast, int columnLast) range) =>
            Create(new Uri(range.text, UriKind.RelativeOrAbsolute), TextPosition.Create(range.lineFirst, range.columnFirst), TextPosition.Create(range.lineLast, range.columnLast));
#endif
    }
}
