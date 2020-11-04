using System;
using System.Diagnostics;

namespace Favalet.Reactive.Disposables
{
    [DebuggerStepThrough]
    public sealed class Disposable : IDisposable
    {
        private Disposable()
        { }

        public void Dispose()
        { }

        public static readonly IDisposable Empty =
            new Disposable();
    }
}
