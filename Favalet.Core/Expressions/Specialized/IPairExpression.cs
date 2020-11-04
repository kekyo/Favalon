using System;
using System.Collections.Generic;
using System.Linq;

namespace Favalet.Expressions.Specialized
{
    public interface IPairExpression :
        IExpression
    {
        IExpression Left { get; }
        IExpression Right { get; }
        
        Type IdentityType { get; }

        IExpression Create(IExpression left, IExpression right);
    }

    public static class PairExpressionExtension
    {
        public static IEnumerable<IExpression> Children(this IPairExpression pair)
        {
            yield return pair.Left;
            yield return pair.Right;
        }
        
        public static IExpression? Create(this IPairExpression pair, IEnumerable<IExpression> children) =>
            children.ToArray() switch
            {
                IExpression[] arr when arr.Length == 2 =>
                    pair.Create(arr[0], arr[1]),
                _ =>
                    throw new InvalidOperationException()
            };
    }
}
