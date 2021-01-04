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
using Favalet.Expressions.Algebraic;
using Favalet.Expressions.Specialized;
using Favalet.Internal;
using Favalet.Ranges;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Favalet.Contexts.Unifiers
{
    public interface ITopology
    {
        string View { get; }
        string Dot { get; }
        string GetDot(string headerLabel);
    }

    [DebuggerStepThrough]
    internal readonly struct Unification
    {
        public readonly IExpression Expression;
        public readonly UnifyDirections Direction;

        private Unification(IExpression expression, UnifyDirections direction)
        {
            this.Expression = expression;
            this.Direction = direction;
        }

        public override string ToString() =>
            $"{(this.Direction switch {UnifyDirections.Forward => "==>", UnifyDirections.Backward => "<==", _ => "===" })} {this.Expression.GetPrettyString(PrettyStringTypes.Minimum)}";

        public static Unification Create(IExpression expression, UnifyDirections direction) =>
            new Unification(expression, direction);
    }

    [DebuggerStepThrough]
    [DebuggerDisplay("{" + nameof(View) + "}")]
    internal sealed class UnifyContext :
        ITopology
    {
#if DEBUG
        private IExpression targetRoot;
#else
        private string targetRootString;
#endif
        private readonly Stack<Dictionary<IPlaceholderTerm, Unification>> scopes =
            new Stack<Dictionary<IPlaceholderTerm, Unification>>();
        private Dictionary<IPlaceholderTerm, Unification> topology =
            new Dictionary<IPlaceholderTerm, Unification>(IdentityTermComparer.Instance);

        public readonly ITypeCalculator TypeCalculator;

        private UnifyContext(ITypeCalculator typeCalculator, IExpression targetRoot)
        {
            this.TypeCalculator = typeCalculator;
#if DEBUG
            this.targetRoot = targetRoot;
#else
            this.targetRootString =
                targetRoot.GetPrettyString(PrettyStringTypes.ReadableAll);
#endif
        }
        
        public void SetTargetRoot(IExpression targetRoot) =>
#if DEBUG
            this.targetRoot = targetRoot;
#else
            this.targetRootString =
                targetRoot.GetPrettyString(PrettyStringTypes.ReadableAll);
#endif
        
        public bool IsAssignable(IExpression expression1, IExpression expression2)
        {
            // expression1 <:> expression2
            var or = this.TypeCalculator.Calculate(
                OrExpression.Create(expression1, expression2, TextRange.Unknown));
            return this.TypeCalculator.Equals(expression1, or) ||
                   this.TypeCalculator.Equals(or, expression2);
        }
        
        public bool IsAssignableFrom(IExpression to, IExpression from)
        {
            // to <: from
            var or = this.TypeCalculator.Calculate(
                OrExpression.Create(to, from, TextRange.Unknown));
            return this.TypeCalculator.Equals(or, from);
        }

        public bool TryLookup(IPlaceholderTerm placeholder, out Unification unification) =>
            this.topology.TryGetValue(placeholder, out unification);

        public IExpression? Resolve(IPlaceholderTerm placeholder)
        {
            var ph = placeholder;
            while (true)
            {
                if (this.topology.TryGetValue(ph, out var unification))
                {
                    if (unification.Expression is IPlaceholderTerm ph2)
                    {
                        ph = ph2;
                        continue;
                    }
                    else
                    {
                        return unification.Expression;
                    }
                }
                else
                {
                    return object.ReferenceEquals(ph, placeholder) ? null : ph;
                }
            }
        }

        public void Set(IPlaceholderTerm placeholder, IExpression expression, UnifyDirections direction)
        {
            Debug.Assert(!placeholder.Equals(expression));
            
            // TODO: copy
            if ((this.scopes.Count >= 1) &&
                object.ReferenceEquals(this.topology, this.scopes.Peek()))
            {
                this.topology = new Dictionary<IPlaceholderTerm, Unification>(this.topology);
            }
            this.topology[placeholder] = Unification.Create(expression, direction);
        }
     
        [DebuggerStepThrough]
        private sealed class Disposer : IDisposable
        {
            private UnifyContext? parent;

            public Disposer(UnifyContext parent) =>
                this.parent = parent;

            public void Dispose()
            {
                if (this.parent != null)
                {
                    this.parent.EndScope();
                    this.parent = null;
                }
            }
        }

        public IDisposable BeginScope()
        {
            this.scopes.Push(this.topology);
            return new Disposer(this);
        }

        public bool Commit(bool commit, ErrorCollections raiseCouldNotUnify)
        {
            if (commit)
            {
                this.scopes.Pop();
                this.scopes.Push(this.topology);
            }
            else if (raiseCouldNotUnify == ErrorCollections.JustRaise)
            {
                throw new InvalidOperationException();
            }
            return commit;
        }

        private void EndScope() =>
            this.topology = this.scopes.Pop();
        
        public string View =>
            StringUtilities.Join(
                System.Environment.NewLine,
                (this.topology ?? this.scopes.Peek()).
                    OrderBy(entry => entry.Key, IdentityTermComparer.Instance).
                    Select(entry =>
                        $"{entry.Key.Symbol} {entry.Value}"));

        public string GetDot(string headerLabel)
        {
            var sb = new StringBuilder();
            sb.AppendLine("digraph topology");
            sb.AppendLine("{");
            sb.AppendLine($"    graph [label=\"{headerLabel}\"];");
            sb.AppendLine();

            /////////////////////////////////////////////////////////////////

            string GetIdentity(IExpression expression) =>
                expression switch
                {
                    IPlaceholderTerm ph => $"ph{ph.Index}",
                    // TODO: IPairExpression _ => $"",
                    _ => $"ex{(uint)expression.GetHashCode()}",
                };
            
            sb.AppendLine("    #nodes");
            foreach (var expression in this.topology.Keys.
                Concat(this.topology.Values.Select(unification => unification.Expression)).
                Distinct().
                OrderBy(expression => expression, OrderedExpressionComparer.Instance))
            {
                var shape = expression switch
                {
                    IPlaceholderTerm _ => "circle",
                    // TODO: IPairExpression _ => $"",
                    _ => "box"
                };
                sb.AppendLine($"    {GetIdentity(expression)} [label=\"{expression.GetPrettyString(PrettyStringTypes.Minimum)}\",shape={shape}];");
            }
            sb.AppendLine();
            
            /////////////////////////////////////////////////////////////////
            
            sb.AppendLine("    #topology");
            foreach (var entry in this.topology.
                OrderBy(entry => entry.Key, OrderedExpressionComparer.Instance))
            {
                switch (entry.Value.Direction)
                {
                    case UnifyDirections.Forward:
                        sb.AppendLine($"    {GetIdentity(entry.Key)} -> {GetIdentity(entry.Value.Expression)};");
                        break;
                    case UnifyDirections.Backward:
                        sb.AppendLine($"    {GetIdentity(entry.Value.Expression)} -> {GetIdentity(entry.Key)};");
                        break;
                    case UnifyDirections.BiDirectional:
                        sb.AppendLine($"    {GetIdentity(entry.Key)} -> {GetIdentity(entry.Value.Expression)} [dir=none];");
                        break;
                }
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        public string Dot =>
#if DEBUG
            this.GetDot(this.targetRoot.GetPrettyString(PrettyStringTypes.ReadableAll));
#else
            this.GetDot(this.targetRootString);
#endif

        public static UnifyContext Create(ITypeCalculator typeCalculator, IExpression targetRoot) =>
            new UnifyContext(typeCalculator, targetRoot);
    }
}
