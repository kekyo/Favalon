using Favalet.Expressions;
using Favalet.Expressions.Specialized;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;

namespace Favalet
{
    internal static class ExpressionAssert
    {
        private sealed class Indexes
        {
            private readonly Dictionary<string, string> indexToPseudoIndex = new Dictionary<string, string>();
            private readonly Dictionary<string, string> pseudoIndexToIndex = new Dictionary<string, string>();

            [DebuggerStepThrough]
            public Indexes()
            { }

            public bool TryGetPseudoIndex(string index, out string? pseudoIndex) =>
                this.indexToPseudoIndex.TryGetValue(index, out pseudoIndex);

            public bool TryGetIndex(string pseudoIndex, out string? index) =>
                this.pseudoIndexToIndex.TryGetValue(pseudoIndex, out index);

            public void Set(string index, string pseudoIndex)
            {
                this.indexToPseudoIndex.Add(index, pseudoIndex);
                this.pseudoIndexToIndex.Add(pseudoIndex, index);
            }
        }

        private enum Results
        {
            Negate,
            Assert,
            Ignore
        }

        private static Results Trap(bool result)
        {
            if (!result && Debugger.IsAttached)
            { 
                // Negate
                Debugger.Break();
            }
            return result ? Results.Assert : Results.Negate;
        }

        private static Results Equals(IExpression lhs, IExpression rhs, Indexes indexes)
        {
            if (object.ReferenceEquals(lhs, rhs))
            {
                return Results.Assert;
            }

            if (lhs is IIdentityTerm lp1 &&
                rhs is PseudoPlaceholderProvider.PseudoPlaceholderTerm rp1)
            {
                if (indexes.TryGetPseudoIndex(lp1.Symbol, out var rpi))
                {
                    return Trap(rpi == rp1.Symbol);
                }
                else
                {
                    if (indexes.TryGetIndex(rp1.Symbol, out var li))
                    {
                        return Trap(li == lp1.Symbol);
                    }
                    else
                    {
                        indexes.Set(lp1.Symbol, rp1.Symbol);
                    }
                    return Results.Assert;
                }
            }
            else if (lhs is PseudoPlaceholderProvider.PseudoPlaceholderTerm lp2 &&
                rhs is IIdentityTerm rp2)
            {
                if (indexes.TryGetPseudoIndex(rp2.Symbol, out var lpi))
                {
                    return Trap(lpi == lp2.Symbol);
                }
                else
                {
                    if (indexes.TryGetIndex(lp2.Symbol, out var ri))
                    {
                        return Trap(ri == rp2.Symbol);
                    }
                    else
                    {
                        indexes.Set(rp2.Symbol, lp2.Symbol);
                    }
                    return Results.Assert;
                }
            }

            switch (lhs, rhs)
            {
                case (UnspecifiedTerm _, _):   // Only expected expression
                case (IFunctionExpression(UnspecifiedTerm _, UnspecifiedTerm _), _):
                    return Results.Ignore;
                case (DeadEndTerm _, _):
                    return Results.Ignore;
                case (FourthTerm _, FourthTerm _):
                    return Results.Ignore;
                case (_, DeadEndTerm _):
                    return Trap(false);
                case (IPairExpression le, IPairExpression re)
                    when le.IdentityType.Equals(re.IdentityType):
                    var p1 = Equals(le.Left, re.Left, indexes);
                    var b1 = Equals(le.Right, re.Right, indexes);
                    if (p1 != Results.Negate && b1 != Results.Negate)
                    {
                        if (p1 == Results.Assert && b1 == Results.Assert)
                        {
                            return Equals(lhs.HigherOrder, rhs.HigherOrder, indexes);
                        }
                        return Results.Assert;
                    }
                    else
                    {
                        return Results.Negate;
                    }
                default:
                    if (Trap(lhs.Equals(rhs)) is Results r &&
                        r != Results.Negate)
                    {
                        if (r == Results.Assert)
                        {
                            return Equals(lhs.HigherOrder, rhs.HigherOrder, indexes);
                        }
                        return Results.Assert;
                    }
                    else
                    {
                        return Results.Negate;
                    }
            }
        }

        [DebuggerStepThrough]
        public static bool Equals(IExpression expected, IExpression actual) =>
            Equals(expected, actual, new Indexes()) != Results.Negate;
    }
}
