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

using Favalet.Contexts;
using Favalet.Expressions.Specialized;
using Favalet.Expressions.Algebraic;
using Favalet.Internal;
using Favalet.Ranges;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace Favalet.Expressions
{
    public interface IVariableTerm :
        IIdentityTerm
    {
        BoundAttributes? Attributes { get; }
    }
    
    public sealed class VariableTerm :
        Expression, IVariableTerm
    {
        private readonly IExpression[]? candidates;
        
        public readonly string Symbol;
        public readonly BoundAttributes? Attributes;

        [DebuggerStepThrough]
        private VariableTerm(
            string symbol,
            IExpression higherOrder,
            BoundAttributes? attributes,
            IExpression[]? candidates,
            TextRange range) :
            base(range)
        {
            this.HigherOrder = higherOrder;
            this.Symbol = symbol;
            this.Attributes = attributes;
            this.candidates = candidates;
        }

        public override IExpression HigherOrder { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string IIdentityTerm.Symbol
        {
            [DebuggerStepThrough]
            get => this.Symbol;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object IIdentityTerm.Identity
        {
            [DebuggerStepThrough]
            get => this.Symbol;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        BoundAttributes? IVariableTerm.Attributes
        {
            [DebuggerStepThrough]
            get => this.Attributes;
        }

        public override int GetHashCode() =>
            this.Symbol.GetHashCode();

        public bool Equals(IIdentityTerm rhs) =>
            this.Symbol.Equals(rhs.Symbol);

        public override bool Equals(IExpression? other) =>
            other is IIdentityTerm rhs && this.Equals(rhs);

        protected override IExpression Transpose(ITransposeContext context)
        {
            Debug.Assert(this.Attributes == null);
            Debug.Assert(this.candidates == null);

            var higherOrder = context.Transpose(this.HigherOrder);

            if (context.LookupVariables(this.Symbol) is ({ } attributes, _))
            {
                return new VariableTerm(
                    this.Symbol,
                    higherOrder,
                    attributes,
                    this.candidates,
                    this.Range);
            }
            else
            {
                return new VariableTerm(
                    this.Symbol,
                    higherOrder,
                    BoundAttributes.PrefixLeftToRight,
                    this.candidates,
                    this.Range);
            }
        }

        protected override IExpression MakeRewritable(IMakeRewritableContext context) =>
            new VariableTerm(
                this.Symbol,
                context.MakeRewritableHigherOrder(this.HigherOrder),
                this.Attributes,
                this.candidates,
                this.Range);
        
        protected override IExpression Infer(IInferContext context)
        {
            if (this.candidates != null)
            {
                return this;
            }

            var higherOrder = context.Infer(this.HigherOrder);
            if (context.LookupVariables(this.Symbol) is { } results)
            {
                var targets = results.Variables.
                    Where(v => !context.TypeCalculator.ExactEquals(this, v.Expression)).
                    Select(v =>
                        (symbolHigherOrder: context.Infer(context.MakeRewritableHigherOrder(v.SymbolHigherOrder)), 
                         expression: context.Infer(context.MakeRewritable(v.Expression)))).
                    Memoize();

                if (targets.Length >= 1)
                {
                    var symbolHigherOrder = LogicalCalculator.ConstructNested(
                        targets.Select(v => v.symbolHigherOrder).Memoize(),
                        OrExpression.Create,
                        this.Range)!;

                    var expressionHigherOrder = LogicalCalculator.ConstructNested(
                        targets.Select(v => v.expression.HigherOrder).Memoize(),
                        OrExpression.Create,
                        this.Range)!;
               
                    context.Unify(symbolHigherOrder, expressionHigherOrder, true);
                    context.Unify(expressionHigherOrder, higherOrder, true);
                }
                
                var candidates = 
                    context.TypeCalculator.SortExpressions(
                        expression => expression.HigherOrder,
                        targets.Select(entry => entry.expression)).
                    Memoize();
                return new VariableTerm(
                    this.Symbol,
                    higherOrder,
                    results.Attributes,
                    candidates,
                    this.Range);
            }

            if (object.ReferenceEquals(this.HigherOrder, higherOrder))
            {
                return this;
            }
            else
            {
                return new VariableTerm(
                    this.Symbol,
                    higherOrder,
                    this.Attributes,
                    this.candidates,
                    this.Range);
            }
        }

        protected override IExpression Fixup(IFixupContext context)
        {
            var higherOrder = context.FixupHigherOrder(this.HigherOrder);

            if (this.candidates is { Length: >= 1 })
            {
                var targets = this.candidates.
                    Select(context.Fixup).
                    Memoize();

                if (targets.Length >= 1)
                {
                    var targetsHigherOrder = LogicalCalculator.ConstructNested(
                        targets.
                            Select(target => target.HigherOrder).
                            Memoize(),
                        OrExpression.Create,
                        this.Range)!;

                    var filterPredicate = context.TypeCalculator.Reduce(
                        AndExpression.Create(
                            higherOrder, targetsHigherOrder,
                            this.Range));

                    var filteredCandidates =
                        context.TypeCalculator.SortExpressions(
                            expression => expression.HigherOrder,
                            targets.
                                Select(target =>
                                    (target,
                                     calculated: context.TypeCalculator.Reduce(
                                        AndExpression.Create(target.HigherOrder, filterPredicate, this.Range)))).
                                Where(entry => entry.calculated.Equals(filterPredicate)).
                                Select(entry => entry.target)).
                        Memoize();

                    if (filteredCandidates.Length >= 1)
                    {
                        return new VariableTerm(
                            this.Symbol,
                            higherOrder,
                            this.Attributes,
                            filteredCandidates, 
                            this.Range);
                    }
                }
            }

            if (object.ReferenceEquals(this.HigherOrder, higherOrder))
            {
                return this;
            }
            else
            {
                return new VariableTerm(
                    this.Symbol,
                    higherOrder,
                    this.Attributes,
                    this.candidates,
                    this.Range);
            }
        }

        protected override IExpression Reduce(IReduceContext context)
        {
            if (this.candidates is { Length: 1 })
            {
                var candidate = this.candidates[0];
                if (candidate is IBoundVariableTerm bound)
                {
                    if (context.LookupVariables(bound.Symbol) is { } results)
                    {
                        var variables = context.TypeCalculator.SortExpressions(
                            expression => expression.HigherOrder,
                            results.Variables
                                .Where(variable =>
                                    context.TypeCalculator.Equals(variable.SymbolHigherOrder, bound.HigherOrder))
                                .Select(variable => variable.Expression)).Memoize();
                        if (variables.Length == 1)
                        {
                            return context.Reduce(variables[0]);
                        }
                        else if (variables.Length >= 2)
                        {
                            return new VariableTerm(
                                this.Symbol,
                                this.HigherOrder,
                                this.Attributes,
                                variables,
                                this.Range);
                        }
                    }
                }
                else
                {
                    return context.Reduce(candidate);
                }
            }

            return this;
        }

        protected override IEnumerable GetXmlValues(IXmlRenderContext context) =>
            new[] { new XAttribute("symbol", this.Symbol) };

        protected override string GetPrettyString(IPrettyStringContext context) =>
            context.FinalizePrettyString(
                this,
                ((context.Type >= PrettyStringTypes.Strict) &&
                    this.Attributes is { }) ?
                    $"{this.Symbol}@{this.Attributes}" :
                    this.Symbol);

        [DebuggerStepThrough]
        public static VariableTerm Create(string symbol, IExpression higherOrder, TextRange range) =>
            new VariableTerm(symbol, higherOrder, default, default, range);
        [DebuggerStepThrough]
        public static VariableTerm Create(string symbol, TextRange range) =>
            new VariableTerm(symbol, UnspecifiedTerm.Instance, default, default, range);
    }

    [DebuggerStepThrough]
    public static class VariableTermExtension
    {
        public static void Deconstruct(
            this IVariableTerm variable,
            out string symbol,
            out BoundAttributes? attributes)
        {
            symbol = variable.Symbol;
            attributes = variable.Attributes;
        }
    }
}
