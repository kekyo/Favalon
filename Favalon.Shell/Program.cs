﻿/////////////////////////////////////////////////////////////////////////////////////////////////
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
using System;
using System.Collections.Generic;
using System.Linq;
using Favalet.Contexts;
using Favalet.Ranges;
using Favalet.Reactive;
using Favalon.Internal;

namespace Favalon
{
    public static class Program
    {
        private static IEnumerable<string> wc(IEnumerable<string> stdin)
        {
            var bc = 0;
            var wc = 0;
            var lc = 0;

            foreach (var line in stdin)
            {
                bc += line.Length;
                wc += line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
                lc++;
            }

            yield return $"{bc},{wc},{lc}";
        }

        private static Environments CreateEnvironments()
        {
            var environments = Environments.Create(CLRTypeCalculator.Instance);
            
            foreach (var type in typeof(object).Assembly.GetTypes().
                Where(type => type.IsPublic && !type.IsNestedPublic && !type.IsGenericType))
            {
                var typeTerm = CLRGenerator.Type(type);
                environments.MutableBind(type.FullName!, TextRange.From(type), typeTerm);
            }

            return environments;
        }

        public static int Main(string[] args)
        {
            var console = InteractiveConsoleHost.Create();

            var lexer = Lexer.Create();
            var uri = new Uri("console", UriKind.RelativeOrAbsolute);
            var tokens = lexer.Analyze(uri, console);

            var parser = CLRParser.Create();
            var parsed = parser.Parse(tokens);
            
            var environments = CreateEnvironments();
            
            IDisposable? d = default;
            d = parsed.Subscribe(Observer.Create<IExpression>(
                expression =>
                {
                    var reduced = environments.Reduce(expression);
                    if (reduced is VariableTerm("exit"))
                    {
                        d?.Dispose();
                    }
                    else
                    {
                        Console.WriteLine(reduced.GetPrettyString(PrettyStringTypes.ReadableAll));
                    }
                },
                ex => { },
                () => { }));

            console.Run();

            return 0;
        }
    }
}
