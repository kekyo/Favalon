using Favalet.Contexts;
using Favalet.Internal;
using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

namespace Favalet.Expressions
{
    public interface IMethodTerm :
        ITerm, ICallableExpression
    {
        MethodBase RuntimeMethod { get; }
    }

    public sealed class MethodTerm :
        Expression, IMethodTerm
    {
        private readonly LazySlim<IExpression> higherOrder;

        public readonly MethodBase RuntimeMethod;

        [DebuggerStepThrough]
        private MethodTerm(MethodBase runtimeMethod, LazySlim<IExpression> higherOrder)
        {
            this.RuntimeMethod = runtimeMethod;
            this.higherOrder = higherOrder;
        }

        public override IExpression HigherOrder
        {
            [DebuggerStepThrough]
            get => this.higherOrder.Value;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        MethodBase IMethodTerm.RuntimeMethod
        {
            [DebuggerStepThrough]
            get => this.RuntimeMethod;
        }

        public override int GetHashCode() =>
            this.RuntimeMethod.GetHashCode();

        public bool Equals(IMethodTerm rhs) =>
            this.RuntimeMethod.Equals(rhs.RuntimeMethod);

        public override bool Equals(IExpression? other) =>
            other is IMethodTerm rhs && this.Equals(rhs);
        
        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            new MethodTerm(
                this.RuntimeMethod,
                LazySlim.Create(context.MakeRewritableHigherOrder(this.HigherOrder)));

        protected override IExpression Infer(IInferContext context) =>
            this;

        protected override IExpression Fixup(IFixupContext context) =>
            this;

        protected override IExpression Reduce(IReduceContext context) =>
            this;

        public IExpression Call(IReduceContext context, IExpression argument)
        {
            if (argument is IConstantTerm constant)
            {
                if (this.RuntimeMethod is ConstructorInfo constructor)
                {
                    var result = constructor.Invoke(new[] { constant.Value });
                    return ConstantTerm.From(result);
                }
                else
                {
                    var method = (MethodInfo)this.RuntimeMethod;
                    var result = method.Invoke(null, new[] { constant.Value });
                    return ConstantTerm.From(result);
                }
            }
            else
            {
                throw new ArgumentException(argument.GetPrettyString(PrettyStringTypes.Readable));
            }
        }

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            new[] { new XAttribute("name", this.RuntimeMethod.GetReadableName()) };

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                this.RuntimeMethod.GetReadableName());

        [DebuggerStepThrough]
        private static IExpression CreateHigherOrder(MethodBase method) =>
            FunctionExpression.Create(
                TypeTerm.From(method.GetParameters()[0].ParameterType),
                TypeTerm.From(
                    method is MethodInfo mi ?
                        mi.ReturnType :
                        method.DeclaringType));

        [DebuggerStepThrough]
        public static MethodTerm From(MethodBase method) =>
            new MethodTerm(method, LazySlim.Create(() => CreateHigherOrder(method)));
    }

    public static class MethodTermExtension
    {
        [DebuggerStepThrough]
        public static void Deconstruct(
            this IMethodTerm method,
            out MethodBase runtimeMethod) =>
            runtimeMethod = method.RuntimeMethod;
    }
}
