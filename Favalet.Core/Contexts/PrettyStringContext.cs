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
using Favalet.Expressions.Specialized;
using System.Diagnostics;

namespace Favalet.Contexts
{
    public interface IPrettyStringContext
    {
        PrettyStringTypes Type { get; }

        string GetPrettyString(IExpression expression);
        string FinalizePrettyString(IExpression expression, string preFormatted);
    }

    public enum PrettyStringTypes
    {
        Minimum,
        Readable,
        ReadableAll,
        Strict,
        StrictAll
    }

    public sealed class PrettyStringContext :
        IPrettyStringContext
    {
        private readonly bool isPartial;

        public readonly PrettyStringTypes Type;

        [DebuggerStepThrough]
        private PrettyStringContext(PrettyStringTypes type, bool isPartial)
        {
            this.Type = type;
            this.isPartial = isPartial;
        }

        PrettyStringTypes IPrettyStringContext.Type
        {
            [DebuggerStepThrough]
            get => this.Type;
        }

        public string GetPrettyString(IExpression expression) =>
            (this.Type, expression, expression) switch
            {
                (PrettyStringTypes.Minimum, Expression expr, _) => expr.InternalGetPrettyString(this),
                (PrettyStringTypes.Readable, Expression expr, _) => expr.InternalGetPrettyString(this),
                (PrettyStringTypes.ReadableAll, Expression expr, _) => expr.InternalGetPrettyString(this),
                (_, Expression expr, DeadEndTerm _) => expr.InternalGetPrettyString(this),
                (_, Expression expr, _) => $"{expr.Type} {expr.InternalGetPrettyString(this)}",
                _ => this.FinalizePrettyString(expression, "?")
            };

        string IPrettyStringContext.GetPrettyString(IExpression expression) =>
            (this.Type, expression, expression) switch
            {
                (PrettyStringTypes.Minimum, Expression expr, ITerm _) => expr.InternalGetPrettyString(this),
                (PrettyStringTypes.Minimum, Expression expr, _) => $"({expr.InternalGetPrettyString(this)})",
                (PrettyStringTypes.Readable, Expression expr, ITerm _) => expr.InternalGetPrettyString(this),
                (PrettyStringTypes.Readable, Expression expr, _) => $"({expr.InternalGetPrettyString(this)})",
                (PrettyStringTypes.ReadableAll, Expression expr, ITerm _) => expr.InternalGetPrettyString(this),
                (PrettyStringTypes.ReadableAll, Expression expr, _) => $"({expr.InternalGetPrettyString(this)})",
                (_, Expression expr, DeadEndTerm _) => expr.InternalGetPrettyString(this),
                (_, Expression expr, _) => $"({expr.Type} {expr.InternalGetPrettyString(this)})",
                _ => this.FinalizePrettyString(expression, "?")
            };

        [DebuggerStepThrough]
        private IPrettyStringContext MakePartial() =>
            (this.Type, this.isPartial) switch
            {
                (PrettyStringTypes.ReadableAll, _) => this,
                (PrettyStringTypes.StrictAll, _) => this,
                _ => new PrettyStringContext(this.Type, true),
            };

        public string FinalizePrettyString(IExpression expression, string preFormatted)
        {
            var higherOrder = expression.HigherOrder;
            return (this.Type, this.isPartial, expression, higherOrder) switch
            {
                (_, true, _, _) =>
                    preFormatted,
                (_, _, _, DeadEndTerm _) =>
                    preFormatted,
                (PrettyStringTypes.Minimum, _, _, _) =>
                    preFormatted, 
                (PrettyStringTypes.Readable, _, _, UnspecifiedTerm _) =>
                    preFormatted,
                (PrettyStringTypes.Readable, _, _, FourthTerm _) =>
                    preFormatted,
                (_, _, ITerm _, _) =>
                    $"{preFormatted}:{this.MakePartial().GetPrettyString(higherOrder)}",
                _ =>
                    $"({preFormatted}):{this.MakePartial().GetPrettyString(higherOrder)}",
            };
        }

        [DebuggerStepThrough]
        public static PrettyStringContext Create(PrettyStringTypes type) =>
            new PrettyStringContext(type, false);
    }
}
