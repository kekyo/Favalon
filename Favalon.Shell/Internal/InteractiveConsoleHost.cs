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
        private readonly StringBuilder line = new StringBuilder();
        private int currentColumn = 0;
        private IObserver<Input>? observer;

        private InteractiveConsoleHost()
        { }

        public override IDisposable Subscribe(IObserver<Input> subscribe)
        {
            Debug.Assert(this.observer == null);
            this.observer = subscribe;
            return new CancellationDisposable(this.cts);
        }

        public void Clear()
        {
            Console.Clear();
            this.line.Clear();
            this.currentColumn = 0;
        }

        public void InputEnter()
        {
            Console.WriteLine();
            for (var index = 0; index < this.line.Length; index++)
            {
                this.observer?.OnNext(this.line[index]);
            }
            this.line.Clear();
            this.currentColumn = 0;
            this.observer?.OnNext(InputTypes.NextLine);
            this.observer?.OnNext(InputTypes.DelimiterHint);
        }

        public bool InputChar(char inch)
        {
            if ((inch == '\r') || (inch == '\n'))   // TODO: sequence
            {
                this.InputEnter();
                return true;
            }
            else if (!char.IsControl(inch))
            {
                this.line.Insert(this.currentColumn, inch);
                if (this.currentColumn == this.line.Length)
                {
                    Console.Write(inch);
                }
                else
                {
                    var left = Console.CursorLeft + 1;
                    var top = Console.CursorTop;
                    Console.Write(this.line.ToString().Substring(this.currentColumn));
                    Console.SetCursorPosition(left, top);
                }
                this.currentColumn++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool InputForward()
        {
            if (this.currentColumn < this.line.Length)
            {
                this.currentColumn++;
                var left = Console.CursorLeft + 1;
                var top = Console.CursorTop;
                Console.SetCursorPosition(left, top);
                return true;
            }
            else
            {
                return false;
            }
        }
  
        public bool InputBackward()
        {
            if (this.currentColumn >= 1)
            {
                this.currentColumn--;
                var left = Console.CursorLeft;
                var top = Console.CursorTop;
                if (left >= 1)
                {
                    left--;
                }
                Console.SetCursorPosition(left, top);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool InputBackspace()
        {
            if (this.currentColumn >= 1)
            {
                line.Remove(this.currentColumn - 1, 1);
                this.currentColumn--;
                var left = Console.CursorLeft;
                var top = Console.CursorTop;
                if (left >= 1)
                {
                    left--;
                }
                Console.SetCursorPosition(left, top);
                Console.Write(line.ToString().Substring(this.currentColumn) + " ");
                Console.SetCursorPosition(left, top);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool InputDelete()
        {
            if (this.currentColumn < line.Length)
            {
                line.Remove(this.currentColumn, 1);
                var left = Console.CursorLeft;
                var top = Console.CursorTop;
                Console.Write(line.ToString().Substring(this.currentColumn) + " ");
                Console.SetCursorPosition(left, top);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Run()
        {
            var readKeyController = new UnsafeReadKeyController(this.cts.Token);
            
            while (true)
            {
                if (!(readKeyController.ReadKey() is ConsoleKeyInfo key))
                {
                    this.observer?.OnCompleted();
                    break;
                }

                switch (key.Key)
                {
                    case ConsoleKey.Delete:
                        this.InputDelete();
                        break;

                    case ConsoleKey.Backspace:
                        this.InputBackspace();
                        break;

                    case ConsoleKey.RightArrow:
                        this.InputForward();
                        break;

                    case ConsoleKey.LeftArrow:
                        this.InputBackward();
                        break;
                    
                    case ConsoleKey.Enter:
                        this.InputEnter();
                        break;

                    default:
                        this.InputChar(key.KeyChar);
                        break;
                }
            }
        }

        public static InteractiveConsoleHost Create() =>
            new InteractiveConsoleHost();
    }
}
