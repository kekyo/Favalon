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
using System.Diagnostics;
using Favalet.Ranges;

namespace Favalet.Tokens
{
    [DebuggerStepThrough]
    public abstract class SymbolToken :
        Token
    {
        private protected SymbolToken(TextRange range) :
            base(range)
        { }

        public abstract char Symbol { get; }

        public override string ToString() =>
            this.Symbol.ToString();

        public void Deconstruct(out char symbol) =>
            symbol = this.Symbol;
        public void Deconstruct(out char symbol, out TextRange range)
        {
            symbol = this.Symbol;
            range = this.Range;
        }
    }
}
