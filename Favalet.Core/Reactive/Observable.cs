/////////////////////////////////////////////////////////////////////////////////////////////////
//
// Favalon - An Interactive Shell Based on a Typed Lambda Calculus.
// Copyright (c) 2018-2020 Kouji Matsui (@kozy_kekyo, @kekyo2)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
/////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Favalet.Reactive.Disposables;

namespace Favalet.Reactive
{
    [DebuggerStepThrough]
    public static class Observable
    {
        [DebuggerStepThrough]
        private sealed class DelegatedObservable<T> :
            IObservable<T>
        {
            private readonly Func<IObserver<T>, IDisposable> subscribe;

            public DelegatedObservable(Func<IObserver<T>, IDisposable> subscribe) =>
                this.subscribe = subscribe;

            public IDisposable Subscribe(IObserver<T> observer) =>
                this.subscribe(observer);
        }
        
        public static IObservable<T> Create<T>(Func<IObserver<T>, IDisposable> subscribe) =>
            new DelegatedObservable<T>(subscribe);

        public static IObservable<T> ToObservable<T>(this IEnumerable<T> enumerable) =>
            new DelegatedObservable<T>(observer =>
            {
                try
                {
                    foreach (var value in enumerable)
                    {
                        observer.OnNext(value);
                    }
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                    return Disposable.Empty;
                }
                observer.OnCompleted();
                return Disposable.Empty;
            });
    }
}

namespace Favalet.Reactive.Linq
{
    [DebuggerStepThrough]
    public static class Observable
    {
        [DebuggerStepThrough]
        public static IEnumerable<T> ToEnumerable<T>(this IObservable<T> observable)
        {
#if NET40
            // TODO: Improvement
            using (var ev = new ManualResetEventSlim())
            {
                var list = new List<T>();
                Exception? ex = null;
                ManualResetEventSlim? evx = ev;
                observable.Subscribe(Observer.Create<T>(
                    list.Add,
                    e =>
                    {
                        ex = e;
                        evx?.Set();
                    },
                    () => evx?.Set()));
                evx.Wait();
                evx = null;
                if (ex != null)
                {
                    throw new System.Reflection.TargetInvocationException(ex);
                }
                return list;
            }
#else
            var coll = new System.Collections.Concurrent.BlockingCollection<(T, System.Runtime.ExceptionServices.ExceptionDispatchInfo?)>();
            observable.Subscribe(Observer.Create<T>(
                value => coll.Add((value, null)),
                ex => coll.Add((default, System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex))),
                () => coll.CompleteAdding()));
            return coll.GetConsumingEnumerable().Select(entry =>
            {
                entry.Item2?.Throw();
                return entry.Item1;
            });
#endif
        }
    }
}
