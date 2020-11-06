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
using System.Text;
using System.Threading;
using Favalet.Lexers;
using Favalet.Reactive;
using Favalet.Reactive.Disposables;

namespace Favalon.Internal
{
    public sealed class InteractiveConsoleHost :
        ObservableBase<Input>
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private IObserver<Input>? observer;

        private InteractiveConsoleHost()
        { }

        public override IDisposable Subscribe(IObserver<Input> subscribe)
        {
            Debug.Assert(this.observer == null);
            this.observer = subscribe;
            return new CancellationDisposable(this.cts);
        }
        
        public void Run()
        {
            var line = new StringBuilder();
            var currentColumn = 0;

            while (true)
            {
                if (cts.IsCancellationRequested)
                {
                    this.observer?.OnCompleted();
                    break;
                }

                if (!Console.KeyAvailable)
                {
                    // Naive method :)
                    Thread.Sleep(100);
                    continue;
                }

                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Delete:
                        if (currentColumn < line.Length)
                        {
                            line.Remove(currentColumn, 1);
                            var left = Console.CursorLeft;
                            var top = Console.CursorTop;
                            Console.Write(line.ToString().Substring(currentColumn));
                            Console.SetCursorPosition(top, left);
                        }
                        break;

                    case ConsoleKey.Backspace:
                        if (currentColumn >= 1)
                        {
                            line.Remove(currentColumn - 1, 1);
                            currentColumn--;
                            var left = Console.CursorLeft;
                            var top = Console.CursorTop;
                            if (left >= 1)
                            {
                                left--;
                            }
                            Console.SetCursorPosition(top, left);
                            Console.Write(line.ToString().Substring(currentColumn) + " ");
                            Console.SetCursorPosition(top, left);
                        }
                        break;

                    case ConsoleKey.RightArrow:
                        if (currentColumn < line.Length)
                        {
                            currentColumn++;
                            var left = Console.CursorLeft + 1;
                            var top = Console.CursorTop;
                            Console.SetCursorPosition(top, left);
                        }
                        break;

                    case ConsoleKey.LeftArrow:
                        if (currentColumn >= 1)
                        {
                            currentColumn--;
                            var left = Console.CursorLeft;
                            var top = Console.CursorTop;
                            if (left >= 1)
                            {
                                left--;
                            }
                            Console.SetCursorPosition(top, left);
                        }
                        break;
                    
                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        for (var index = 0; index < line.Length; index++)
                        {
                            this.observer?.OnNext(line[index]);
                        }
                        line.Clear();
                        this.observer?.OnNext(InputTypes.NextLine);
                        break;
                    
                    default:
                        line.Insert(currentColumn, key.KeyChar);
                        Console.Write(key.KeyChar);
                        break;
                }
            }
        }

        public static InteractiveConsoleHost Create() =>
            new InteractiveConsoleHost();
    }
}