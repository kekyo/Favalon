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

using Favalet.Internal;
using Favalet.Ranges;
using System;
using System.Diagnostics;
using System.Text;

namespace Favalet.Lexers
{
    [DebuggerStepThrough]
    internal sealed class LexRunnerContext
    {
        public readonly Uri Uri;

        private TextPosition start;
        private int currentLine;
        private int currentColumn;
        private readonly StringBuilder tokenBuffer = new();

        private LexRunnerContext(Uri uri) =>
            this.Uri = uri;
        
        public Input? StringLastInput { get; private set; }

        public void SetStringLastInput(Input? input) =>
            this.StringLastInput = input;

        public void Append(char inch)
        {
            this.currentColumn++;
            this.tokenBuffer.Append(inch);
        }

        public (string tokenText, TextRange range) GetTokenTextAndClear()
        {
            var tokenText = this.tokenBuffer.ToString();
            this.tokenBuffer.Clear();
            
            var first = this.start;
            var last = TextPosition.Create(this.currentLine, this.currentColumn);
            
            this.currentColumn++;
            this.start = TextPosition.Create(this.currentLine, this.currentColumn);
            
            return (tokenText, TextRange.Create(this.Uri, first, last));
        }

        public void ForwardOnly()
        {
            this.currentColumn++;
        }

        public void ForwardNextLine()
        {
            this.currentColumn = 0;
            this.currentLine++;
        }

        public TextRange GetRangeAndClear()
        {
            Debug.Assert(this.tokenBuffer.Length == 0);

            var first = this.start;
            var last = TextPosition.Create(this.currentLine, this.currentColumn);

            this.start = last;

            return TextRange.Create(this.Uri, first, last);
        }

        public static LexRunnerContext Create(Uri uri) =>
            new LexRunnerContext(uri);
    }
}
