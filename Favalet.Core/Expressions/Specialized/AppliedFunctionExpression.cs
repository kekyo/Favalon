using System.Diagnostics;

namespace Favalet.Expressions.Specialized
{
    internal interface IAppliedFunctionExpression :
        IFunctionExpression
    {
    }

    [DebuggerStepThrough]
    internal sealed class AppliedFunctionExpression :
        FunctionExpressionBase, IAppliedFunctionExpression
    {
        #region Factory
        [DebuggerStepThrough]
        private sealed class AppliedFunctionExpressionFactory :
            FunctionExpressionFactoryBase
        {
            private AppliedFunctionExpressionFactory()
            {
            }

            protected override IFunctionExpression OnCreate(
                IExpression parameter, IExpression result, IExpression higherOrder) =>
                new AppliedFunctionExpression(parameter, result, higherOrder);

            public static readonly AppliedFunctionExpressionFactory Instance =
                new AppliedFunctionExpressionFactory();
        }
        #endregion

        private protected override FunctionExpressionFactoryBase Factory =>
            AppliedFunctionExpressionFactory.Instance;
        
        private AppliedFunctionExpression(
            IExpression parameter, IExpression result, IExpression higherOrder) :
            base(parameter, result, higherOrder)
        {
        }
 
        public static AppliedFunctionExpression Create(
            IExpression parameter, IExpression result) =>
            (AppliedFunctionExpression)AppliedFunctionExpressionFactory.Instance.Create(
                parameter, result);
    }
}
