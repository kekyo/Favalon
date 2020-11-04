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

using Favalet.Expressions.Algebraic;
using System;
using Favalet.Contexts;
using Favalet.Expressions;

namespace Favalet
{
    public interface ITypeCalculator :
        ILogicalCalculator
    {
    }
    
    public class TypeCalculator :
        LogicalCalculator, ITypeCalculator
    {
        protected override ChoiceResults ChoiceForAnd(
            IExpression left, IExpression right)
        {
            // Function variance:
            if (left is IFunctionExpression(IExpression lp, IExpression lr) &&
                right is IFunctionExpression(IExpression rp, IExpression rr))
            {
                var parameter = this.ChoiceForAnd(lp, rp);
                var result = this.ChoiceForAnd(lr, rr);

                // Contravariance.
                switch (parameter, result)
                {
                    case (ChoiceResults.Equal, ChoiceResults.Equal):
                        return ChoiceResults.Equal;

                    case (ChoiceResults.Equal, ChoiceResults.AcceptLeft):
                    case (ChoiceResults.AcceptLeft, ChoiceResults.Equal):
                    case (ChoiceResults.AcceptLeft, ChoiceResults.AcceptLeft):
                        return ChoiceResults.AcceptLeft;
                    
                    case (ChoiceResults.Equal, ChoiceResults.AcceptRight):
                    case (ChoiceResults.AcceptRight, ChoiceResults.Equal):
                    case (ChoiceResults.AcceptRight, ChoiceResults.AcceptRight):
                        return ChoiceResults.AcceptRight;
                }
            }

            return base.ChoiceForAnd(left, right);
        }

        protected override ChoiceResults ChoiceForOr(
            IExpression left, IExpression right)
        {
            // Function variance:
            if (left is IFunctionExpression(IExpression lp, IExpression lr) &&
                right is IFunctionExpression(IExpression rp, IExpression rr))
            {
                var parameter = this.ChoiceForOr(lp, rp);
                var result = this.ChoiceForOr(lr, rr);
                
                // Covariance.
                switch (parameter, result)
                {
                    case (ChoiceResults.Equal, ChoiceResults.Equal):
                        return ChoiceResults.Equal;

                    case (ChoiceResults.Equal, ChoiceResults.AcceptLeft):
                    case (ChoiceResults.AcceptLeft, ChoiceResults.Equal):
                    case (ChoiceResults.AcceptLeft, ChoiceResults.AcceptLeft):
                        return ChoiceResults.AcceptLeft;
                    
                    case (ChoiceResults.Equal, ChoiceResults.AcceptRight):
                    case (ChoiceResults.AcceptRight, ChoiceResults.Equal):
                    case (ChoiceResults.AcceptRight, ChoiceResults.AcceptRight):
                        return ChoiceResults.AcceptRight;
                }
            }

            return base.ChoiceForOr(left, right);
        }

        public new static readonly TypeCalculator Instance =
            new TypeCalculator();
    }
}