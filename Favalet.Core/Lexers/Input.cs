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

namespace Favalet.Lexers
{
    [Flags]
    public enum InputTypes
    {
        UnicodeCharacter = 0x00,
        NextLine = 0x01,
        DelimiterHint = 0x02
    }
    
    [DebuggerStepThrough]
    public readonly struct Input
    {
        private readonly char inch;
        private readonly InputTypes type;

        private Input(char inch, InputTypes type)
        {
            this.inch = inch;
            this.type = type;
        }

        public bool IsNextLine =>
            (this.type & InputTypes.NextLine) == InputTypes.NextLine;
        public bool IsDelimiterHint =>
            (this.type & InputTypes.DelimiterHint) == InputTypes.DelimiterHint;
        
        public override string ToString() =>
            this.type switch
            {
                InputTypes.UnicodeCharacter => $"'{this.inch}'",
                _ => $"[{this.type}]"
            };

        public static Input Create(char inch) =>
            new Input(inch, InputTypes.UnicodeCharacter);
        public static Input Create(InputTypes type) =>
            new Input('\0', type);

        public static implicit operator char(Input input)
        {
            Debug.Assert(input.type == InputTypes.UnicodeCharacter);
            return input.inch;
        }
        public static implicit operator InputTypes(Input input) =>
            input.type;
        public static implicit operator Input(char inch) =>
            new Input(inch, InputTypes.UnicodeCharacter);
        public static implicit operator Input(InputTypes type) =>
            new Input('\0', type);
    }
}
