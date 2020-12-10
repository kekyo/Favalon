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

namespace Favalon.Console
{
    public interface IConsole :
        IDisposable
    {
        int ColumnPosition { get; }
        
        void ClearScreen();
        
        void Write(char ch);
        void Write(string str);
        void WriteLine();
        void WriteLine(string str);
        
        void SetColumnPosition(int column);

        IDisposable PushColor(ConsoleColor newColor);

        void Alarm();
        
        ConsoleKeyInfo ReadKey(CancellationToken token);
    }

    public sealed class CLRConsole : IConsole
    {
        private readonly Queue<ConsoleKeyInfo> queue = new Queue<ConsoleKeyInfo>();
        private readonly ManualResetEventSlim gate = new ManualResetEventSlim();
        private readonly Thread thread;
        private readonly Stack<ConsoleColor> colors = new Stack<ConsoleColor>();
        private volatile bool abort;

        public CLRConsole()
        {
            this.thread = new Thread(() =>
            {
                while (!this.abort)
                {
                    var keyInfo = System.Console.ReadKey(true);
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

        public void Dispose() =>
            this.abort = true;

        public int ColumnPosition =>
            System.Console.CursorLeft;

        public void ClearScreen() =>
            System.Console.Clear();

        public void Write(char ch) =>
            System.Console.Write(ch);
        public void Write(string str) =>
            System.Console.Write(str);
        public void WriteLine() =>
            System.Console.WriteLine();
        public void WriteLine(string str) =>
            System.Console.WriteLine(str);

        public void SetColumnPosition(int column) =>
            System.Console.SetCursorPosition(column, System.Console.CursorTop);

        private sealed class ColorDisposable : IDisposable
        {
            private CLRConsole? parent;

            public ColorDisposable(CLRConsole parent) =>
                this.parent = parent;

            public void Dispose()
            {
                if (this.parent != null)
                {
                    System.Console.ForegroundColor = this.parent.colors.Pop();
                    this.parent = null;
                }
            }
        }
        
        public IDisposable PushColor(ConsoleColor newColor)
        {
            this.colors.Push(System.Console.ForegroundColor);
            System.Console.ForegroundColor = newColor;
            return new ColorDisposable(this);
        }

        public void Alarm() =>
            System.Console.Beep();

        public ConsoleKeyInfo ReadKey(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                this.gate.Wait(token);

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

        public static CLRConsole Create() =>
            new CLRConsole();
    }
}
