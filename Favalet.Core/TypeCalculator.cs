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
using Favalet.Expressions.Specialized;

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
                if (left is ILambdaExpression({ } lp, { } lr) &&
                    right is ILambdaExpression({ } rp, { } rr))
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
                if (left is ILambdaExpression({ } lp, { } lr) &&
                    right is ILambdaExpression({ } rp, { } rr))
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
        
        public override IExpression Reduce(IExpression operand, IExpressionChoicer choicer)
        {
            // Normalize
            if (operand is ILambdaExpression lambda)
            {
                var parameter = this.Reduce(lambda.Parameter, choicer);
                var body = this.Reduce(lambda.Body, choicer);
                
                switch (parameter, body)
                {
                    case (IBinaryExpression pb, IBinaryExpression bb):
                        return ((IPairExpression) pb).Create(
                            ((IPairExpression) bb).Create(
                                ((IPairExpression) lambda).Create(pb.Left, bb.Left, bb.Range),
                                ((IPairExpression) lambda).Create(pb.Left, bb.Right, bb.Range),
                                lambda.Range),
                            ((IPairExpression) bb).Create(
                                ((IPairExpression) lambda).Create(pb.Right, bb.Left, bb.Range),
                                ((IPairExpression) lambda).Create(pb.Right, bb.Right, bb.Range),
                                lambda.Range),
                            lambda.Range);
                    case (_, IBinaryExpression bb):
                        return ((IPairExpression) bb).Create(
                            ((IPairExpression) lambda).Create(lambda.Parameter, bb.Left, bb.Range),
                            ((IPairExpression) lambda).Create(lambda.Parameter, bb.Right, bb.Range),
                            lambda.Range);
                    case (IBinaryExpression pb, _):
                        return ((IPairExpression) pb).Create(
                            ((IPairExpression) lambda).Create(pb.Left, lambda.Body, pb.Range),
                            ((IPairExpression) lambda).Create(pb.Right, lambda.Body, pb.Range),
                            lambda.Range);
                    default:
                        if (object.ReferenceEquals(lambda.Parameter, parameter) &&
                            object.ReferenceEquals(lambda.Body, body))
                        {
                            return lambda;
                        }
                        else
                        {
                            return ((IPairExpression) lambda).Create(parameter, body, lambda.Range);
                        }
                }
            }

            return base.Reduce(operand, choicer);
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