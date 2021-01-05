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

using Favalet;
using Favalet.Expressions;
using Favalet.Contexts;
using Favalet.Reactive;
using Favalon.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Favalon
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            // Step 1: Building reactive console host.
            var console = CLRConsole.Create();

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                console.WriteLine($"fash [{Thread.CurrentThread.ManagedThreadId}/{(Task.CurrentId?.ToString() ?? "?")}] : {e.ExceptionObject}");

            var consoleHost = InteractiveConsoleHost.Create(
                console, "fash> ");

            // Step 2: Building reactive lexer.
            var lexer = Lexer.Create();
            var uri = new Uri("console", UriKind.RelativeOrAbsolute);
            var tokens = lexer.Analyze(uri, consoleHost);

            // Step 3: Building reactive parser.
            var parser = CLRParser.Create();
            var parsed = parser.Parse(tokens);
            
            // Step 4: Create type environment.
            var environments = CLREnvironments.Create();
            
            // Step 5: Add some shell related commands.
            environments.MutableBindMethod("reset", new Action(environments.Reset));
            environments.MutableBindMethod("clear", new Action(consoleHost.ClearScreen));
            environments.MutableBindMethod("exit", new Action(consoleHost.ShutdownAsynchronously));

            // TODO: test
            environments.MutableBindTypeAndMembers(typeof(Test));

            // Step 6: Building final receiver.
            using (parsed.Subscribe(
                // We will get something parsed expression.
                Observer.Create<IExpression>(expression =>
                {
                    try
                    {
                        // Step 7: Reduce the expression.
                        var reduced = environments.Reduce(expression);
                        
                        // Step 8: Render result.
                        switch (reduced)
                        {
                            case IConstantTerm({ } value)
                                when value.GetType().IsPrimitive || value is string:
                                console.WriteLine(value.ToString()!);
                                break;
                            case IConstantTerm(IEnumerable<string> lines):
                                foreach (var line in lines)
                                {
                                    console.WriteLine(line);
                                }
                                break;
                            default:
                                console.WriteLine(reduced.GetPrettyString(PrettyStringTypes.Readable));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        using (console.PushColor(ConsoleColor.Red))
                        {
                            try
                            {
                                foreach (var message in ex.GetReadableString())
                                {
                                    console.WriteLine(message);
                                }
                            }
                            catch (Exception ex2)
                            {
                                Trace.WriteLine(ex2);
                            }
                        }
                    }
                },
                ex => { },
                () => { })))
            {
                // Step 8: Run reactive pipelines.
                consoleHost.Run();
            }

            return 0;
        }
    }
}
