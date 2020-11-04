using Favalet.Expressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Favalet.Internal
{
    [DebuggerStepThrough]
    internal sealed class ReferenceComparer :
        IEqualityComparer<IExpression>,
        IComparer<IExpression>
    {
        public bool Equals(IExpression x, IExpression y) =>
            object.ReferenceEquals(x, y);

        public int GetHashCode(IExpression obj) =>
            RuntimeHelpers.GetHashCode(obj);

        public int Compare(IExpression x, IExpression y) =>
            RuntimeHelpers.GetHashCode(x).CompareTo(RuntimeHelpers.GetHashCode(y));

        public static readonly ReferenceComparer Instance =
            new ReferenceComparer();
    }
}
