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
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Favalet.Internal
{
    [DebuggerStepThrough]
    internal static class SharpSymbols
    {
        public static readonly Dictionary<Type, string> ReadableTypeNames = new()
        {
            { typeof(void), "unit" },
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(char), "char" },
            { typeof(string), "string" },
            { typeof(object), "object" },
            { typeof(IntPtr), "nint" },
            { typeof(UIntPtr), "nuint" },
            { typeof(IEnumerable<>), "seq" },
        };
        
        public static readonly Dictionary<string, (string symbol, BoundAttributes attributes)> OperatorSymbols = new()
        {
            { "op_Addition", ("+", BoundAttributes.InfixLeftToRight(BoundPrecedences.Addition)) },
            { "op_Subtraction", ("-", BoundAttributes.InfixLeftToRight(BoundPrecedences.Addition)) },
            { "op_Multiply", ("*", BoundAttributes.InfixLeftToRight(BoundPrecedences.Multiply)) },
            { "op_Division", ("/", BoundAttributes.InfixLeftToRight(BoundPrecedences.Multiply)) },
            { "op_Append", ("+", BoundAttributes.InfixLeftToRight(BoundPrecedences.Addition)) },
            { "op_Concatenate", ("+", BoundAttributes.InfixLeftToRight(BoundPrecedences.Addition)) },
            { "op_Modulus", ("%", BoundAttributes.InfixLeftToRight(BoundPrecedences.Multiply)) },
            { "op_BitwiseAnd", ("&", BoundAttributes.InfixLeftToRight(BoundPrecedences.BitOperators)) },
            { "op_BitwiseOr", ("|", BoundAttributes.InfixLeftToRight(BoundPrecedences.BitOperators)) },
            { "op_LogicalAnd", ("&&", BoundAttributes.InfixLeftToRight(BoundPrecedences.LogicalOperators)) },
            { "op_LogicalOr", ("||", BoundAttributes.InfixLeftToRight(BoundPrecedences.LogicalOperators)) },
            { "op_ExclusiveOr", ("^", BoundAttributes.InfixLeftToRight(BoundPrecedences.BitOperators)) },
            { "op_LeftShift", ("<<", BoundAttributes.InfixLeftToRight(BoundPrecedences.BitOperators)) },
            { "op_RightShift", (">>", BoundAttributes.InfixLeftToRight(BoundPrecedences.BitOperators)) },
            //{ "op_LogicalNot", ("!", BoundAttributes.InfixLeftToRight(BoundPrecedences.PrefixOperators)) },    // TODO: duplicate prefix/infix symbols
            //{ "op_UnaryPlus", ("+", BoundAttributes.InfixLeftToRight(BoundPrecedences.PrefixOperators)) },
            //{ "op_UnaryNegation", ("-", BoundAttributes.InfixLeftToRight(BoundPrecedences.PrefixOperators)) },
            { "op_Equality", ("==", BoundAttributes.InfixLeftToRight(BoundPrecedences.Comparer)) },
            { "op_Inequality", ("!=", BoundAttributes.InfixLeftToRight(BoundPrecedences.Comparer)) },
            { "op_LessThanOrEqual", ("<=", BoundAttributes.InfixLeftToRight(BoundPrecedences.Comparer)) },
            { "op_GreaterThanOrEqual", (">=", BoundAttributes.InfixLeftToRight(BoundPrecedences.Comparer)) },
            { "op_LessThan", ("<", BoundAttributes.InfixLeftToRight(BoundPrecedences.Comparer)) },
            { "op_GreaterThan", (">", BoundAttributes.InfixLeftToRight(BoundPrecedences.Comparer)) },
            { "op_PipeRight", ("|>", BoundAttributes.InfixLeftToRight(BoundPrecedences.Composer)) },
            { "op_PipeLeft", ("<|", BoundAttributes.InfixLeftToRight(BoundPrecedences.Composer)) },
            { "op_Dereference", ("!", BoundAttributes.InfixLeftToRight(BoundPrecedences.PrefixOperators)) },
            { "op_ComposeRight", (">>", BoundAttributes.InfixLeftToRight(BoundPrecedences.Composer)) },
            { "op_ComposeLeft", ("<<", BoundAttributes.InfixLeftToRight(BoundPrecedences.Composer)) },
            { "op_AdditionAssignment", ("+=", BoundAttributes.InfixLeftToRight(BoundPrecedences.Binder)) },
            { "op_SubtractionAssignment", ("-=", BoundAttributes.InfixLeftToRight(BoundPrecedences.Binder)) },
            { "op_MultiplyAssignment", ("*=", BoundAttributes.InfixLeftToRight(BoundPrecedences.Binder)) },
            { "op_DivisionAssignment", ("/=", BoundAttributes.InfixLeftToRight(BoundPrecedences.Binder)) },
        };
    }
}
