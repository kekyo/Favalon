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
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Favalon.Console
{
    public enum InputModifiers
    {
        Char,
        Word,
        Line
    }

    public interface IInteractiveConsole
    {
        void ClearScreen();
        
        void InputEnter();
        bool InputChar(char inch);
        bool InputForward(InputModifiers modifier = InputModifiers.Char);
        bool InputBackward(InputModifiers modifier = InputModifiers.Char);
        bool InputBackspace(InputModifiers modifier = InputModifiers.Char);
        bool InputDelete(InputModifiers modifier = InputModifiers.Char);
        bool InputOlder();
        bool InputNewer();
    }

    public abstract class InteractiveConsole :
        IInteractiveConsole
    {
        private readonly IConsole console;

        private readonly List<string> history = new List<string>();
        private readonly StringBuilder line = new StringBuilder();
        private int currentColumn = 0;
        private int historyIndex = 0;
        private string prompt;
        
        protected InteractiveConsole(IConsole console, string prompt)
        {
            this.console = console;
            this.prompt = prompt;
        }

        protected abstract void OnArrivalInput(Input input);

        protected ConsoleKeyInfo ReadKey(CancellationToken token) =>
            this.console.ReadKey(token);

        protected void WritePrompt() =>
            this.console.Write(this.prompt);

        public void ClearScreen()
        {
            this.console.ClearScreen();
            this.line.Clear();
            this.currentColumn = 0;
        }

        public void InputEnter()
        {
            this.console.WriteLine();

            var line = this.line.ToString();
            this.line.Clear();

            if (!string.IsNullOrWhiteSpace(line))
            {
                this.history.Add(line);

                foreach (var inch in line)
                {
                    this.OnArrivalInput(inch);
                }
            }
            
            this.currentColumn = 0;
            this.OnArrivalInput(InputTypes.NextLine);
            this.OnArrivalInput(InputTypes.DelimiterHint);
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
                    this.console.Write(inch);
                }
                else
                {
                    var left = this.console.ColumnPosition + 1;
                    this.console.Write(this.line.ToString().Substring(this.currentColumn));
                    this.console.SetColumnPosition(left);
                }
                this.currentColumn++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool InputForward(InputModifiers modifier = InputModifiers.Char)
        {
            if (this.currentColumn < this.line.Length)
            {
                if (modifier == InputModifiers.Line)
                {
                    var differ = this.line.Length - this.currentColumn;
                    this.currentColumn += differ;
                    var left = this.console.ColumnPosition + differ;
                    this.console.SetColumnPosition(left);
                }
                else
                {
                    this.currentColumn++;
                    var left = this.console.ColumnPosition + 1;
                    this.console.SetColumnPosition(left);
                }
                return true;
            }
            else
            {
                this.console.Alarm();
                return false;
            }
        }
  
        public bool InputBackward(InputModifiers modifier = InputModifiers.Char)
        {
            if (this.currentColumn >= 1)
            {
                if (modifier == InputModifiers.Line)
                {
                    var left = this.console.ColumnPosition - this.currentColumn;
                    this.currentColumn = 0;
                    this.console.SetColumnPosition(left);
                }
                else
                {
                    this.currentColumn--;
                    var left = this.console.ColumnPosition;
                    if (left >= 1)
                    {
                        left--;
                    }
                    this.console.SetColumnPosition(left);
                }
                return true;
            }
            else
            {
                this.console.Alarm();
                return false;
            }
        }

        public bool InputBackspace(InputModifiers modifier = InputModifiers.Char)
        {
            if (this.currentColumn >= 1)
            {
                line.Remove(this.currentColumn - 1, 1);
                this.currentColumn--;
                var left = this.console.ColumnPosition;
                if (left >= 1)
                {
                    left--;
                }
                this.console.SetColumnPosition(left);
                this.console.Write(line.ToString().Substring(this.currentColumn) + " ");
                this.console.SetColumnPosition(left);
                return true;
            }
            else
            {
                this.console.Alarm();
                return false;
            }
        }

        public bool InputDelete(InputModifiers modifier = InputModifiers.Char)
        {
            if (this.currentColumn < line.Length)
            {
                line.Remove(this.currentColumn, 1);
                var left = this.console.ColumnPosition;
                this.console.Write(line.ToString().Substring(this.currentColumn) + " ");
                this.console.SetColumnPosition(left);
                return true;
            }
            else
            {
                this.console.Alarm();
                return false;
            }
        }

        public bool InputOlder()
        {
            if (this.historyIndex < this.history.Count)
            {
                this.console.SetColumnPosition(this.prompt.Length);
                this.console.Write(new string(' ', this.line.Length));
                this.console.SetColumnPosition(this.prompt.Length);

                this.historyIndex++;

                var line = this.history[this.history.Count - this.historyIndex];
                this.line.Clear();
                this.line.Append(line);
                
                this.console.Write(line);
                this.currentColumn = line.Length;
                
                return true;
            }
            else
            {
                this.console.Alarm();
                return false;
            }
        }

        public bool InputNewer()
        {
            if (this.historyIndex >= 2)
            {
                this.console.SetColumnPosition(this.prompt.Length);
                this.console.Write(new string(' ', this.line.Length));
                this.console.SetColumnPosition(this.prompt.Length);

                this.historyIndex--;

                var line = this.history[this.history.Count - this.historyIndex];
                this.line.Clear();
                this.line.Append(line);
                
                this.console.Write(line);
                this.currentColumn = line.Length;
                
                return true;
            }
            else if (this.historyIndex == 1)
            {
                this.console.SetColumnPosition(this.prompt.Length);
                this.console.Write(new string(' ', this.line.Length));
                this.console.SetColumnPosition(this.prompt.Length);

                this.historyIndex--;

                this.line.Clear();

                this.currentColumn = 0;
                return true;
            }
            else
            {
                this.console.Alarm();
                return false;
            }
        }

        protected static InputModifiers GetInputModifier(ConsoleModifiers modifier) =>
            modifier switch
            {
                ConsoleModifiers.Shift => InputModifiers.Word,
                ConsoleModifiers.Control => InputModifiers.Line,
                _ => InputModifiers.Char
            };
    }
}
