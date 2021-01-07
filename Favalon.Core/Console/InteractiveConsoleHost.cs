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

using Favalet.Lexers;
using Favalet.Reactive.Disposables;
using System;
using System.Diagnostics;
using System.Threading;

namespace Favalon.Console
{
    public interface IInteractiveHost :
        IObservable<Input>
    {
        void ShutdownAsynchronously();
    }
    
    public sealed class InteractiveConsoleHost :
        InteractiveConsole, IInteractiveHost
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private IObserver<Input>? observer;

        private InteractiveConsoleHost(IConsole console, string prompt) :
            base(console, prompt)
        {
        }

        public IDisposable Subscribe(IObserver<Input> subscribe)
        {
            Debug.Assert(this.observer == null);
            this.observer = subscribe;
            return new DelegatedDisposable(this.ShutdownAsynchronously);
        }

        protected override void OnArrivalInput(Input input) =>
            this.observer?.OnNext(input);

        public void ShutdownAsynchronously() =>
            this.cts.Cancel();

        public void Run()
        {
            this.WritePrompt();

            try
            {
                while (!this.cts.IsCancellationRequested)
                {
                    var key = this.ReadKey(this.cts.Token);
                    switch (key.Key)
                    {
                        case ConsoleKey.Delete:
                            this.InputDelete(GetInputModifier(key.Modifiers));
                            break;
                        case ConsoleKey.Backspace:
                            this.InputBackspace(GetInputModifier(key.Modifiers));
                            break;
                        case ConsoleKey.RightArrow:
                            this.InputForward(GetInputModifier(key.Modifiers));
                            break;
                        case ConsoleKey.LeftArrow:
                            this.InputBackward(GetInputModifier(key.Modifiers));
                            break;
                        case ConsoleKey.UpArrow:
                            this.InputOlder();
                            break;
                        case ConsoleKey.DownArrow:
                            this.InputNewer();
                            break;
                        case ConsoleKey.Enter:
                            this.InputEnter();
                            if (!this.cts.IsCancellationRequested)
                            {
                                this.WritePrompt();
                            }
                            break;
                        case ConsoleKey.C when key.Modifiers == ConsoleModifiers.Control:
                        case (ConsoleKey)3:   // VK_CANCEL
                            this.ResetLine();
                            if (!this.cts.IsCancellationRequested)
                            {
                                this.WritePrompt();
                            }
                            break;

                        default:
                            this.InputChar(key.KeyChar);
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                this.observer?.OnCompleted();
            }
        }

        public static InteractiveConsoleHost Create(IConsole console, string prompt) =>
            new InteractiveConsoleHost(console, prompt);
    }
}
