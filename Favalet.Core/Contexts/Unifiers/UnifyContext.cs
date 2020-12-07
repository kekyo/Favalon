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
using Favalet.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Favalet.Contexts.Unifiers
{
    public interface ITopology
    {
        string View { get; }
    }

    [DebuggerStepThrough]
    internal struct Unification
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

    [DebuggerDisplay("{View}")]
    internal sealed class UnifyContext :
        ITopology
    {
        private IExpression targetRoot;

        private readonly Stack<Dictionary<IPlaceholderTerm, Unification>> scopes =
            new Stack<Dictionary<IPlaceholderTerm, Unification>>();
        private Dictionary<IPlaceholderTerm, Unification> topology =
            new Dictionary<IPlaceholderTerm, Unification>(IdentityTermComparer.Instance);

        public readonly ITypeCalculator TypeCalculator;

        [DebuggerStepThrough]
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
        
        [DebuggerStepThrough]
        public void SetTargetRoot(IExpression targetRoot) =>
#if DEBUG
            this.targetRoot = targetRoot;
#else
            this.targetRootString =
                targetRoot.GetPrettyString(PrettyStringTypes.ReadableAll);
#endif

        [DebuggerStepThrough]
        public bool TryLookup(IPlaceholderTerm placeholder, out Unification unification) =>
            this.topology.TryGetValue(placeholder, out unification);

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
        public IDisposable BeginScope()
        {
            this.scopes.Push(this.topology);
            return new Disposer(this);
        }

        [DebuggerStepThrough]
        public bool Commit(bool commit, bool raiseCouldNotUnify)
        {
            if (commit)
            {
                this.scopes.Pop();
                this.scopes.Push(this.topology);
            }
            else if (raiseCouldNotUnify)
            {
                throw new InvalidOperationException();
            }
            return commit;
        }

        [DebuggerStepThrough]
        private void EndScope() =>
            this.topology = this.scopes.Pop();
        
        public string View
        {
            [DebuggerStepThrough]
            get => StringUtilities.Join(
                System.Environment.NewLine,
                (this.topology ?? this.scopes.Peek()).
                    OrderBy(entry => entry.Key, IdentityTermComparer.Instance).
                    Select(entry =>
                        $"{entry.Key.Symbol} {entry.Value}"));
        }

        [DebuggerStepThrough]
        public static UnifyContext Create(ITypeCalculator typeCalculator, IExpression targetRoot) =>
            new UnifyContext(typeCalculator, targetRoot);
    }
}
