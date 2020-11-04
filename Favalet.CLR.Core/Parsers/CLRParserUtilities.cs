using System;
using Favalet.Expressions;
using Favalet.Tokens;

namespace Favalet.Parsers
{
    internal static class CLRParserUtilities
    {
        public static void SetNumericalSign(
            CLRParseRunnerContext context,
            NumericalSignToken numericSign) =>
            context.PreSignToken = numericSign;
        
        public static void CombineNumericValue(
            CLRParseRunnerContext context,
            NumericToken numeric)
        {
            var sign = context.PreSignToken?.Sign == NumericalSignes.Minus ? -1 : 1;
            if (int.TryParse(numeric.Value, out var intValue))
            {
                context.CombineAfter(ConstantTerm.From(intValue * sign));
                context.PreSignToken = null;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Couldn't parse numeric: {numeric.Value}");
            }
        }
    }
}
