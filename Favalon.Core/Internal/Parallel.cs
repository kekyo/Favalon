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

#if NET35

using System.Collections.Generic;

namespace System.Threading.Tasks
{
    internal static class Parallel
    {
        public static void ForEach<T>(
            IEnumerable<T> enumerable,
            Action<T> predicate)
        {
            using (var waiter = new ManualResetEvent(false))
            {
                var max = Environment.ProcessorCount * 2;
                using (var semaphore = new Semaphore(max, max))
                {
                    var count = 1;
                    foreach (var value in enumerable)
                    {
                        semaphore.WaitOne();

                        Interlocked.Increment(ref count);
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            try
                            {
                                predicate(value);
                            }
                            finally
                            {
                                semaphore.Release();
                                if (Interlocked.Decrement(ref count) <= 0)
                                {
                                    waiter.Set();
                                }
                            }
                        }, null);
                    }
                    if (Interlocked.Decrement(ref count) <= 0)
                    {
                        waiter.Set();
                    }

                    waiter.WaitOne();
                }
            }
        }
    }
}

#endif
