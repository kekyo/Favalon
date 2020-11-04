using System;
using System.Diagnostics;

namespace Favalet.Reactive
{
    public static class Observer
    {
        [DebuggerStepThrough]
        private sealed class DelegatedObserver<T> :
            IObserver<T>
        {
            private readonly Action<T> onNext;
            private readonly Action<Exception> onError;
            private readonly Action onCompleted;

            public DelegatedObserver(
                Action<T> onNext,
                Action<Exception> onError,
                Action onCompleted)
            {
                this.onNext = onNext;
                this.onError = onError;
                this.onCompleted = onCompleted;
            }

            public void OnNext(T value) =>
                this.onNext(value);

            public void OnError(Exception error) =>
                this.onError(error);

            public void OnCompleted() =>
                this.onCompleted();
        }

        [DebuggerStepThrough]
        public static IObserver<T> Create<T>(
            Action<T> onNext,
            Action<Exception> onError,
            Action onCompleted) =>
            new DelegatedObserver<T>(onNext, onError, onCompleted);
    }
}
