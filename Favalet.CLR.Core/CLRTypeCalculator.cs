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
using Favalet.Expressions.Algebraic;
using Favalet.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Favalet
{
    public sealed class CLRTypeCalculator :
        TypeCalculator
    {
        [DebuggerStepThrough]
        public sealed class CLRTypeCalculatorChoicer :
            TypeCalculatorChoicer
        {
            private CLRTypeCalculatorChoicer()
            { }

            public override ChoiceResults ChoiceForAnd(
                ILogicalCalculator calculator,
                IExpression left, IExpression right)
            {
                // Narrowing
                if (left is ITypeTerm(Type lt) &&
                    right is ITypeTerm(Type rt))
                {
                    var rtl = lt.IsAssignableFrom(rt);
                    var ltr = rt.IsAssignableFrom(lt);

                    switch (rtl, ltr)
                    {
                        case (true, true):
                            return ChoiceResults.Equal;
                        case (true, false):
                            return ChoiceResults.AcceptRight;
                        case (false, true):
                            return ChoiceResults.AcceptLeft;
                    }
                }

                return base.ChoiceForAnd(calculator, left, right);
            }

            public override ChoiceResults ChoiceForOr(
                ILogicalCalculator calculator,
                IExpression left, IExpression right)
            {
                // Widening
                if (left is ITypeTerm(Type lt) &&
                    right is ITypeTerm(Type rt))
                {
                    var rtl = lt.IsAssignableFrom(rt);
                    var ltr = rt.IsAssignableFrom(lt);

                    switch (rtl, ltr)
                    {
                        case (true, true):
                            return ChoiceResults.Equal;
                        case (true, false):
                            return ChoiceResults.AcceptLeft;
                        case (false, true):
                            return ChoiceResults.AcceptRight;
                    }
                }

                return base.ChoiceForOr(calculator, left, right);
            }
         
            public new static readonly CLRTypeCalculatorChoicer Instance =
                new CLRTypeCalculatorChoicer();
        }
        
        public override IChoicer DefaultChoicer =>
            CLRTypeCalculatorChoicer.Instance;

        protected override IComparer<IExpression>? Sorter =>
            OrderedTypeTermComparer.Instance;

        public new static readonly CLRTypeCalculator Instance =
            new CLRTypeCalculator();
    }
}
