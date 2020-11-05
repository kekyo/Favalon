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
using Favalet.Reactive;
using Favalet.Reactive.Disposables;
using Favalet.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Favalet.Ranges;

namespace Favalet
{
    public interface ILexer
    {
        IObservable<Token> Analyze(Uri uri, IObservable<Input> chars);
    }
    
    public sealed class Lexer : ILexer
    {
        [DebuggerStepThrough]
        private Lexer()
        {
        }

        [DebuggerStepThrough]
        public IObservable<Token> Analyze(Uri uri, IObservable<Input> inputs) =>
            Observable.Create<Token>(observer =>
            {
                var context = LexRunnerContext.Create(uri);
                var runner = WaitingRunner.Instance;

                return inputs.Subscribe(Observer.Create<Input>(
                    input =>
                    {
                        switch (runner.Run(context, input))
                        {
                            case LexRunnerResult(LexRunner next, Token token0, Token token1):
                                observer.OnNext(token0);
                                observer.OnNext(token1);
                                runner = next;
                                break;
                            case LexRunnerResult(LexRunner next, Token token, _):
                                observer.OnNext(token);
                                runner = next;
                                break;
                            case LexRunnerResult(LexRunner next, _, _):
                                runner = next;
                                break;
                        }
                    },
                    observer.OnError,
                    () =>
                    {
                        if (runner.Finish(context) is LexRunnerResult(_, Token finalToken, _))
                        {
                            observer.OnNext(finalToken);
                        }
                        observer.OnCompleted();
                    }));
            });
     
        [DebuggerStepThrough]
        public static Lexer Create() =>
            new Lexer();
    }

    [DebuggerStepThrough]
    public static class LexerExtension
    {
        public static IObservable<Token> Analyze(this ILexer lexer, IEnumerable<Input> inputs) =>
            lexer.Analyze(TextRange.UnknownUri, inputs.ToObservable());
        public static IObservable<Token> Analyze(this ILexer lexer, Uri uri, IEnumerable<Input> inputs) =>
            lexer.Analyze(uri, inputs.ToObservable());

        public static IObservable<Token> Analyze(this ILexer lexer, IEnumerable<char> chars) =>
            lexer.Analyze(TextRange.UnknownUri, chars.Select(Input.Create));
        public static IObservable<Token> Analyze(this ILexer lexer, Uri uri, IEnumerable<char> chars) =>
            lexer.Analyze(uri, chars.Select(Input.Create));

        public static IObservable<Token> Analyze(this ILexer lexer, TextReader tr) =>
            lexer.Analyze(TextRange.UnknownUri, tr);
        public static IObservable<Token> Analyze(this ILexer lexer, Uri uri, TextReader tr) =>
            lexer.Analyze(uri, Observable.Create<Input>(observer =>
            {
                while (true)
                {
                    var inch = tr.Read();
                    if (inch < 0)
                    {
                        break;
                    }
                    observer.OnNext((char)inch);
                }
                observer.OnCompleted();
                return Disposable.Empty;
            }));
    }
}
