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
    internal sealed class CLRApplyingRunner : ApplyingRunner
    {
        [DebuggerStepThrough]
        private CLRApplyingRunner()
        { }

        public override ParseRunnerResult Run(
            ParseRunnerContext context,
            ParseRunnerFactory factory,
            Token token)
        {
            Debug.Assert(context is CLRParseRunnerContext);
            Debug.Assert(context.Current != null);
            Debug.Assert(((CLRParseRunnerContext)context).PreSignToken == null);

            switch (token)
            {
                case NumericToken numeric:
                    CLRParserUtilities.CombineNumericValue(
                        (CLRParseRunnerContext)context!,
                        numeric);
                    return ParseRunnerResult.Empty(factory.Applying);

                case NumericalSignToken numericSign:
                    // "abc -" / "123 -" ==> binary op or signed
                    if (context!.LastToken is WhiteSpaceToken)
                    {
                        CLRParserUtilities.SetNumericalSign(
                            (CLRParseRunnerContext)context,
                            numericSign);
                        return ParseRunnerResult.Empty(NumericalSignedRunner.Instance);
                    }
                    // "abc-" / "123-" / "(abc)-" ==> binary op
                    else
                    {
                        context.CombineAfter(
                            VariableTerm.Create(numericSign.Symbol.ToString()));
                        return ParseRunnerResult.Empty(this);
                    }

                default:
                    return base.Run(context!, factory, token);
            }
        }

        internal new static readonly ParseRunner Instance =
            new CLRApplyingRunner();
    }
}
