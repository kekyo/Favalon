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

using Favalet.Expressions;
using Favalet.Tokens;
using System;
using System.Diagnostics;

namespace Favalet.Parsers
{
    public class ApplyingRunner : ParseRunner
    {
        [DebuggerStepThrough]
        protected ApplyingRunner()
        { }

        public override ParseRunnerResult Run(
            ParseRunnerContext context,
            ParseRunnerFactory factory,
            Token token)
        {
            Debug.Assert(context.Current != null);
            //Debug.Assert(context.PreSignToken == null);

            switch (token)
            {
                case WhiteSpaceToken _:
                    return ParseRunnerResult.Empty(this);
                
                case IdentityToken identity:
                    context.CombineAfter(VariableTerm.Create(identity.Identity));
                    return ParseRunnerResult.Empty(factory.Applying);

                case OpenParenthesisToken parenthesis:
                    context.PushScope(parenthesis.Pair);
                    return ParseRunnerResult.Empty(factory.Waiting);
                
                case CloseParenthesisToken parenthesis:
                    context.PopScope(parenthesis.Pair);
                    return ParseRunnerResult.Empty(factory.Applying);

                default:
                    throw new InvalidOperationException(token.ToString());
            }
        }

        internal static readonly ParseRunner Instance =
            new ApplyingRunner();
    }
}
