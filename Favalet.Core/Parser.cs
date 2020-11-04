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

                    Debug.WriteLine($"{index - 1}: '{token}': {context}");

                    context.SetLastToken(token);
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

        public static readonly Parser Instance =
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
