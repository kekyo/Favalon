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
using Favalet.Expressions;
using Favalet.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Favalet
{
    public interface ITypeCalculator :
        ILogicalCalculator
    {
        IExpressionChoicer DefaultChoicer { get; }
        
        IEnumerable<IExpression> SortExpressions(
            Func<IExpression, IExpression> selector,
            IEnumerable<IExpression> enumerable);
    }
    
    public class TypeCalculator :
        LogicalCalculator, ITypeCalculator
    {
        [DebuggerStepThrough]
        public class TypeCalculatorChoicer :
            LogicalCalculatorChoicer
        {
            protected TypeCalculatorChoicer()
            { }

            public override ChoiceResults ChoiceForAnd(
                ILogicalCalculator calculator,
                IExpressionChoicer self,
                IExpression left, IExpression right)
            {
                // Function variance:
                if (left is IFunctionExpression(IExpression lp, IExpression lr) &&
                    right is IFunctionExpression(IExpression rp, IExpression rr))
                {
                    var parameter = self.ChoiceForAnd(calculator, self, lp, rp);
                    var result = self.ChoiceForAnd(calculator, self, lr, rr);

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

                return base.ChoiceForAnd(calculator, self, left, right);
            }

            public override ChoiceResults ChoiceForOr(
                ILogicalCalculator calculator,
                IExpressionChoicer self,
                IExpression left, IExpression right)
            {
                // Function variance:
                if (left is IFunctionExpression(IExpression lp, IExpression lr) &&
                    right is IFunctionExpression(IExpression rp, IExpression rr))
                {
                    var parameter = self.ChoiceForOr(calculator, self, lp, rp);
                    var result = self.ChoiceForOr(calculator, self, lr, rr);
                
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

                return base.ChoiceForOr(calculator, self, left, right);
            }
         
            public new static readonly TypeCalculatorChoicer Instance =
                new TypeCalculatorChoicer();
        }

        public override IExpressionChoicer DefaultChoicer =>
            TypeCalculatorChoicer.Instance;

        protected override IComparer<IExpression>? Sorter =>
            OrderedExpressionComparer.Instance;

        IEnumerable<IExpression> ITypeCalculator.SortExpressions(
            Func<IExpression, IExpression> selector,
            IEnumerable<IExpression> enumerable) =>
            base.SortExpressions(selector, enumerable);

        public new static readonly TypeCalculator Instance =
            new TypeCalculator();
    }
}