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
