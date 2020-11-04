using System;
using System.Reflection;
using Favalet.Contexts;
using Favalet.Expressions;
using Favalet.Internal;

namespace Favalet
{
    public sealed class CLRTypeCalculator :
        TypeCalculator
    {
        protected override ChoiceResults ChoiceForAnd(
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

            return base.ChoiceForAnd(left, right);
        }

        protected override ChoiceResults ChoiceForOr(
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

            return base.ChoiceForOr(left, right);
        }

        public new static readonly CLRTypeCalculator Instance =
            new CLRTypeCalculator();
    }
}
