using Favalet.Expressions;
using Favalet.Expressions.Algebraic;
using Favalet.Expressions.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Favalet
{
    [DebuggerStepThrough]
    public static class Generator
    {
        public static Environments Environment() =>
            Favalet.Environments.Create(TypeCalculator.Instance);

        internal static readonly VariableTerm kind =
            VariableTerm.Create("*", FourthTerm.Instance);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static VariableTerm Kind() =>
            kind;
        public static VariableTerm Kind(string symbol) =>
            VariableTerm.Create(symbol, FourthTerm.Instance);

#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static UnspecifiedTerm Unspecified() =>
            UnspecifiedTerm.Instance;

        public static VariableTerm Variable(string symbol) =>
            VariableTerm.Create(symbol);
        public static VariableTerm Variable(string symbol, IExpression higherOrder) =>
            VariableTerm.Create(symbol, higherOrder);

        public static BoundVariableTerm BoundVariable(string symbol) =>
            BoundVariableTerm.Create(symbol);
        public static BoundVariableTerm BoundVariable(string symbol, IExpression higherOrder) =>
            BoundVariableTerm.Create(symbol, higherOrder);

        public static LogicalExpression Logical(IBinaryExpression operand) =>
            LogicalExpression.Create(operand);
#if !NET35 && !NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static LogicalOperator Logical() =>
            LogicalOperator.Instance;

        public static AndExpression And(IExpression lhs, IExpression rhs) =>
            AndExpression.Create(lhs, rhs);
        public static AndExpression And(IExpression lhs, IExpression rhs, IExpression higherOrder) =>
            AndExpression.Create(lhs, rhs, higherOrder);

        public static IExpression? AndExpressions(params IExpression[] expressions) =>
            LogicalCalculator.ConstructNested(expressions, AndExpression.Create);

        public static OrExpression Or(IExpression lhs, IExpression rhs) =>
            OrExpression.Create(lhs, rhs);
        public static OrExpression Or(IExpression lhs, IExpression rhs, IExpression higherOrder) =>
            OrExpression.Create(lhs, rhs, higherOrder);

        public static IExpression? OrExpressions(params IExpression[] expressions) =>
            LogicalCalculator.ConstructNested(expressions, OrExpression.Create);

        public static LambdaExpression Lambda(
            IBoundVariableTerm parameter, IExpression body) =>
            LambdaExpression.Create(parameter, body);
        public static LambdaExpression Lambda(
            string parameter, IExpression body) =>
            LambdaExpression.Create(BoundVariableTerm.Create(parameter), body);
        public static LambdaExpression Lambda(
            IBoundVariableTerm parameter, IExpression body, IFunctionExpression higherOrder) =>
            LambdaExpression.Create(parameter, body, higherOrder);
        public static LambdaExpression Lambda(
            string parameter, IExpression body, IFunctionExpression higherOrder) =>
            LambdaExpression.Create(BoundVariableTerm.Create(parameter), body, higherOrder);

        public static ApplyExpression Apply(
            IExpression function, IExpression argument) =>
            ApplyExpression.Create(function, argument);
        public static ApplyExpression Apply(
            IExpression function, IExpression argument, IExpression higherOrder) =>
            ApplyExpression.Create(function, argument, higherOrder);

        public static FunctionExpression Function(
            IExpression parameter, IExpression result) =>
            FunctionExpression.Create(parameter, result);
        public static FunctionExpression Function(
            IExpression parameter, IExpression result, IFunctionExpression higherOrder) =>
            FunctionExpression.Create(parameter, result, higherOrder);
    }
}
