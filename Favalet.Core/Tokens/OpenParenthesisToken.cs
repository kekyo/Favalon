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

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Favalet.Ranges;

namespace Favalet.Tokens
{
    [DebuggerStepThrough]
    public sealed class OpenParenthesisToken :
        ParenthesisToken
    {
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private OpenParenthesisToken(ParenthesisPair parenthesis, TextRange range) :
            base(parenthesis, range)
        { }

        public override char Symbol =>
            this.Pair.Open;
 
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static OpenParenthesisToken Create(ParenthesisPair parenthesis, TextRange range) =>
            new OpenParenthesisToken(parenthesis, range);
    }
}
