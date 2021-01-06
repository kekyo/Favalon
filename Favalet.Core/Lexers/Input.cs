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
using System.Runtime.CompilerServices;

namespace Favalet.Lexers
{
    [Flags]
    public enum InputTypes
    {
        UnicodeCharacter = 0x00,
        NextLine = 0x01,
        DelimiterHint = 0x02,
        Reset = 0x04
    }
    
    [DebuggerStepThrough]
    public readonly struct Input :
        IEquatable<Input>
    {
        private readonly char inch;
        private readonly InputTypes type;

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private Input(char inch, InputTypes type)
        {
            this.inch = inch;
            this.type = type;
        }

        public bool IsNextLine
        {
#if !NET35 && !NET40
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => (this.type & InputTypes.NextLine) == InputTypes.NextLine;
        }
        
        public bool IsDelimiterHint
        {
#if !NET35 && !NET40
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => (this.type & InputTypes.DelimiterHint) == InputTypes.DelimiterHint;
        }
        
        public bool IsReset
        {
#if !NET35 && !NET40
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => (this.type & InputTypes.Reset) == InputTypes.Reset;
        }

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override int GetHashCode() =>
            base.GetHashCode();

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Equals(char inch) =>
            (this.type == InputTypes.UnicodeCharacter) && (inch == this.inch);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool Equals(Input input) =>
            (this.inch == input.inch) && (this.type == input.type);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        bool IEquatable<Input>.Equals(Input input) =>
            this.Equals(input);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override bool Equals(object? obj) =>
            obj is Input input && this.Equals(input);

        public override string ToString() =>
            this.type switch
            {
                InputTypes.UnicodeCharacter => $"'{this.inch}'",
                _ => $"[{this.type}]"
            };

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Input Create(char inch) =>
            new Input(inch, InputTypes.UnicodeCharacter);
        
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Input Create(InputTypes type) =>
            new Input('\0', type);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator char(Input input)
        {
            Debug.Assert(input.type == InputTypes.UnicodeCharacter);
            return input.inch;
        }
        
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator InputTypes(Input input) =>
            input.type;

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Input(char inch) =>
            new Input(inch, InputTypes.UnicodeCharacter);
        
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Input(InputTypes type) =>
            new Input('\0', type);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool operator ==(Input lhs, char rhs) =>
            lhs.Equals(rhs);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool operator ==(char lhs, Input rhs) =>
            rhs.Equals(lhs);
        
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool operator !=(Input lhs, char rhs) =>
            !lhs.Equals(rhs);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool operator !=(char lhs, Input rhs) =>
            !rhs.Equals(lhs);
    }
}
