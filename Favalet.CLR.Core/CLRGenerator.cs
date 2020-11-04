using Favalet.Expressions;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Favalet
{
    [DebuggerStepThrough]
    public static class CLRGenerator
    {
        public static Environments CLREnvironment(
#if DEBUG
            bool saveLastTopology = true
#else
            bool saveLastTopology = false
#endif
            ) =>
            Favalet.Environments.Create(CLRTypeCalculator.Instance, saveLastTopology);

        public static ITerm Type<T>() =>
            TypeTerm.From(typeof(T));
        public static ITerm Type(Type runtimeType) =>
            TypeTerm.From(runtimeType);

        public static MethodTerm Method(MethodBase runtimeMethod) =>
            MethodTerm.From(runtimeMethod);

        public static ConstantTerm Constant(object value) =>
            ConstantTerm.From(value);
    }
}
