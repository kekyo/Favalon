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
