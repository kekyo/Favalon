using Favalet.Lexers;
using Favalet.Reactive;
using Favalet.Reactive.Disposables;
using Favalet.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Favalet
{
    public interface ILexer
    {
        IObservable<Token> Analyze(IObservable<char> chars);
    }
    
    public sealed class Lexer : ILexer
    {
        [DebuggerStepThrough]
        private Lexer()
        {
        }

        [DebuggerStepThrough]
        public IObservable<Token> Analyze(IObservable<char> chars) =>
            Observable.Create<Token>(observer =>
            {
                var context = LexRunnerContext.Create();
                var runner = WaitingIgnoreSpaceRunner.Instance;

                return chars.Subscribe(Observer.Create<char>(
                    inch =>
                    {
                        switch (runner.Run(context, inch))
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
        public static IObservable<Token> Analyze(this ILexer lexer, IEnumerable<char> chars) =>
            lexer.Analyze(chars.ToObservable());

        public static IObservable<Token> Analyze(this ILexer lexer, TextReader tr) =>
            lexer.Analyze(Observable.Create<char>(observer =>
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
