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

using Favalet.Expressions;
using Favalet.Parsers;
using Favalet.Reactive;
using Favalet.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Favalet
{
    public interface IParser
    {
        IObservable<IExpression> Parse(IObservable<Token> tokens);
    }
    
    public class Parser : IParser
    {
#if DEBUG
        public int BreakIndex = -1;
#endif
        private readonly ParseRunnerFactory factory;

        [DebuggerStepThrough]
        protected Parser(ParseRunnerFactory factory) =>
            this.factory = factory;

        public IObservable<IExpression> Parse(IObservable<Token> tokens) =>
            Observable.Create<IExpression>(observer =>
        {
#if DEBUG
            var index = 0;
#endif
            var context = this.factory.CreateContext();
            var runner = factory.Waiting;

            return tokens.Subscribe(Observer.Create<Token>(
                token =>
                {
#if DEBUG
                    if (index == BreakIndex) Debugger.Break();
                    index++;
#endif
                    if (token is DelimiterHintToken)
                    {
                        // TODO: improves by type decls (how?)
                        if ((context.NestedScopeCount == 0) &&
                            context.Current is IExpression currentTerm)
                        {
                            observer.OnNext(currentTerm);
                            context.ClearCurrent();
                            runner = factory.Waiting;
                        }
                    }
                    else
                    {
                        switch (runner.Run(context, factory, token))
                        {
                            case ParseRunnerResult(ParseRunner next, IExpression expression):
                                observer.OnNext(expression);
                                runner = next;
                                break;
                            case ParseRunnerResult(ParseRunner next, _):
                                runner = next;
                                break;
                        }
                    }

                    context.SetLastToken(token);
#if DEBUG
                    Debug.WriteLine($"{index - 1}: '{token}': {context}");
#endif
                },
                observer.OnError,
                () =>
                {
                    // Contains final result
                    if (context.Current is IExpression finalTerm)
                    {
                        observer.OnNext(finalTerm);
                    }
                    observer.OnCompleted();
                }));
        });

        public static Parser Create() =>
            new Parser(ParseRunnerFactory.Instance);
    }

    public static class ParserExtension
    {
        public static IObservable<IExpression> Parse(
            this IParser parser,
            IEnumerable<Token> tokens) =>
            parser.Parse(tokens.ToObservable());
    }
}
