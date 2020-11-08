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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Favalet.Expressions;
using Favalet.Expressions.Specialized;

namespace Favalet.Internal
{
    internal sealed class OrderedTypeTermComparer :
        OrderedExpressionComparer
    {
        private static readonly Dictionary<Type, int> priorities =
            new Dictionary<Type, int>
            {
                { typeof(byte), 10 },
                { typeof(short), 11 },
                { typeof(int), 12 },
                { typeof(long), 13 },
                { typeof(sbyte), 20 },
                { typeof(ushort), 21 },
                { typeof(uint), 22 },
                { typeof(ulong), 23 },
                { typeof(bool), 30 },
                { typeof(char), 31 },
                { typeof(float), 32 },
                { typeof(double), 33 },
            };
        
        private OrderedTypeTermComparer()
        { }

        // Table 2: Overload Resolution Priority for Return Types
        private static int GetPriority(Type type)
        {
            if (type.IsPrimitive())
            {
                if (priorities.TryGetValue(type, out var r))
                {
                    return r;
                }
            }
            else if (type == typeof(string))
            {
                return 40;
            }
            else if (type.IsEnum())
            {
                return 50;
            }
            
            if (type.IsValueType())
            {
                return 60;
            }
            else if (type.IsClass())
            {
                return 70;
            }
            else if (type.IsInterface())
            {
                return 80;
            }
            else
            {
                return 90;
            }
        }

        public override int Compare(IExpression x, IExpression y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return 0;
            }

            switch (x, y)
            {
                case (ITypeTerm(Type tx), ITypeTerm(Type ty)):
                    var rx = GetPriority(tx);
                    var ry = GetPriority(ty);
                    var r = rx - ry;
                    if (r == 0)
                    {
                        if (tx.IsAssignableFrom(ty))
                        {
                            return -1;
                        }
                        else // if (ty.IsAssignableFrom(tx))
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        return r;
                    }

                default:
                    return base.Compare(x, y);
            }
        }

        public new static readonly OrderedTypeTermComparer Instance =
            new OrderedTypeTermComparer();
    }
}
