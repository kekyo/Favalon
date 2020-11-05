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
        public readonly char Inch;
        public readonly InputTypes Type;

        private Input(char inch, InputTypes type)
        {
            this.Inch = inch;
            this.Type = type;
        }

        public override string ToString() =>
            this.Type switch
            {
                InputTypes.UnicodeCharacter => $"'{this.Inch}'",
                _ => $"[{this.Type}]"
            };

        public static Input Create(char inch) =>
            new Input(inch, InputTypes.UnicodeCharacter);
        public static Input Create(InputTypes type) =>
            new Input('\0', type);

        public static implicit operator char(Input input) =>
            input.Inch;
        public static implicit operator InputTypes(Input input) =>
            input.Type;
        public static implicit operator Input(char inch) =>
            new Input(inch, InputTypes.UnicodeCharacter);
        public static implicit operator Input(InputTypes type) =>
            new Input('\0', type);
    }
}
