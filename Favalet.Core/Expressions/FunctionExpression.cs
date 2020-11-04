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

namespace Favalet.Expressions
{
    [DebuggerStepThrough]
    public sealed class FunctionExpression :
        FunctionExpressionBase
    {
        #region Factory
        [DebuggerStepThrough]
        private sealed class FunctionExpressionFactory :
            FunctionExpressionFactoryBase
        {
            private FunctionExpressionFactory()
            {
            }

            protected override IFunctionExpression OnCreate(
                IExpression parameter, IExpression result, IExpression higherOrder) =>
                new FunctionExpression(parameter, result, higherOrder);

            public static readonly FunctionExpressionFactory Instance =
                new FunctionExpressionFactory();
        }
        #endregion

        private protected override FunctionExpressionFactoryBase Factory =>
            FunctionExpressionFactory.Instance;
        
        private FunctionExpression(
            IExpression parameter, IExpression result, IExpression higherOrder) :
            base(parameter, result, higherOrder)
        {
        }
        
        public static FunctionExpression Create(
            IExpression parameter, IExpression result, IFunctionExpression higherOrder) =>
            (FunctionExpression)FunctionExpressionFactory.Instance.Create(
                parameter, result, higherOrder);

        public static FunctionExpression Create(
            IExpression parameter, IExpression result) =>
            (FunctionExpression)FunctionExpressionFactory.Instance.Create(
                parameter, result);
    }
}
