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

namespace Favalet.Ranges
{
    [DebuggerStepThrough]
    public readonly struct TextRange
    {
        public static readonly Uri UnknownUri =
            new Uri("https://favalon.org/unknown.fv", UriKind.RelativeOrAbsolute);
        public static readonly TextRange Unknown =
            new TextRange(UnknownUri, TextPosition.Zero, TextPosition.Zero);

        public static readonly Uri InternalUri =
            new Uri("https://favalon.org/internal", UriKind.RelativeOrAbsolute);
        public static readonly TextRange Internal =
            new TextRange(InternalUri, TextPosition.Zero, TextPosition.Zero);

        public readonly Uri Uri;
        public readonly TextPosition First;
        public readonly TextPosition Last;

        private TextRange(Uri uri, TextPosition first, TextPosition last)
        {
            this.Uri = uri;
            this.First = first;
            this.Last = last;
        }

        public bool Contains(TextRange inside) =>
            this.Uri.Equals(inside.Uri) &&
            ((this.First.Line < inside.First.Line) || ((this.First.Line == inside.First.Line) && (this.First.Column <= inside.First.Column))) &&
            ((inside.Last.Line < this.Last.Line) || ((inside.Last.Line == this.Last.Line) && (inside.Last.Column <= this.Last.Column)));

        public bool Overlaps(TextRange rhs) =>
            this.Uri.Equals(rhs.Uri) &&
            (this.First <= rhs.Last) && (this.Last >= rhs.First);

        public TextRange Combine(TextRange range) =>
            this.Uri.Equals(range.Uri) ?
                this.Combine(range.First, range.Last) :
                Unknown;
        public TextRange Combine(TextPosition first, TextPosition last) =>
            new TextRange(
                this.Uri,
                (this.First < first) ? this.First : first,
                (this.Last < last) ? this.Last : last);

        public TextRange Subtract(TextRange range) =>
            this.Uri.Equals(range.Uri) ?
                this.Subtract(range.First, range.Last) :
                Unknown;
        public TextRange Subtract(TextPosition first, TextPosition last) =>
            new TextRange(
                this.Uri,
                this.Contains(Create(this.Uri, first)) ? first : this.First,
                this.Contains(Create(this.Uri, last)) ? last : this.Last);

        public string NormalizedText =>
            this.Uri switch
            {
                _ when this.Uri.Equals(UnknownUri) => string.Empty,
                _ when this.Uri.Equals(InternalUri) => "[internal]",
                _ => this.Uri.ToString()
            };
        
        public override string ToString() =>
            (this.First.Equals(this.Last)) ?
                $"{this.NormalizedText}({this.First})" :
                $"{this.NormalizedText}({this.First},{this.Last})";

        public void Deconstruct(out Uri uri, out TextPosition first, out TextPosition last)
        {
            uri = this.Uri;
            first = this.First;
            last = this.Last;
        }
        public void Deconstruct(out Uri uri, out int lineFirst, out int columnFirst, out int lineLast, out int columnLast)
        {
            uri = this.Uri;
            lineFirst = this.First.Line;
            columnFirst = this.First.Column;
            lineLast = this.Last.Line;
            columnLast = this.Last.Column;
        }

        public static TextRange Create(Uri uri, TextPosition position) =>
            new TextRange(uri, position, position);
        public static TextRange Create(Uri uri, TextPosition first, TextPosition last) =>
            new TextRange(uri, first, last);

        public static implicit operator TextRange((int line, int column) position) =>
            Create(UnknownUri, TextPosition.Create(position.line, position.column));
        public static implicit operator TextRange((int lineFirst, int columnFirst, int lineLast, int columnLast) range) =>
            Create(UnknownUri, TextPosition.Create(range.lineFirst, range.columnFirst), TextPosition.Create(range.lineLast, range.columnLast));
        public static implicit operator TextRange((Uri uri, int line, int column) position) =>
            Create(position.uri, TextPosition.Create(position.line, position.column));
        public static implicit operator TextRange((Uri uri, int lineFirst, int columnFirst, int lineLast, int columnLast) range) =>
            Create(range.uri, TextPosition.Create(range.lineFirst, range.columnFirst), TextPosition.Create(range.lineLast, range.columnLast));
        public static implicit operator TextRange((string uri, int line, int column) position) =>
            Create(new Uri(position.uri, UriKind.RelativeOrAbsolute), TextPosition.Create(position.line, position.column));
        public static implicit operator TextRange((string uri, int lineFirst, int columnFirst, int lineLast, int columnLast) range) =>
            Create(new Uri(range.uri, UriKind.RelativeOrAbsolute), TextPosition.Create(range.lineFirst, range.columnFirst), TextPosition.Create(range.lineLast, range.columnLast));
    }
}
