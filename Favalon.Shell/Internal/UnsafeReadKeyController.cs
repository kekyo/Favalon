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
using System.Threading;

namespace Favalon.Internal
{
    internal sealed class UnsafeReadKeyController
    {
        private readonly Queue<ConsoleKeyInfo> queue = new Queue<ConsoleKeyInfo>();
        private readonly ManualResetEventSlim gate = new ManualResetEventSlim();
        private readonly CancellationToken token;
        private readonly Thread thread;

        public UnsafeReadKeyController(CancellationToken token)
        {
            this.token = token;
            this.thread = new Thread(() =>
            {
                while (!this.token.IsCancellationRequested)
                {
                    var keyInfo = Console.ReadKey(true);
                    lock (this.queue)
                    {
                        this.queue.Enqueue(keyInfo);
                        if (this.queue.Count == 1)
                        {
                            this.gate.Set();
                        }
                    }
                }
            });
            this.thread.IsBackground = true;
            this.thread.Start();
        }

        public ConsoleKeyInfo? ReadKey()
        {
            try
            {
                while (true)
                {
                    this.gate.Wait(this.token);
                    lock (this.queue)
                    {
                        if (this.queue.Count >= 1)
                        {
                            var keyInfo = this.queue.Dequeue();
                            if (this.queue.Count == 0)
                            {
                                this.gate.Reset();
                            }
                            return keyInfo;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }
    }
}