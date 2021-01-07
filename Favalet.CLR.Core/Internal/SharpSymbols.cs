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
        
        public static readonly Dictionary<string, (string symbol, bool isInfix)> OperatorSymbols = new()
        {
            { "op_Addition", ("+", true) },
            { "op_Subtraction", ("-", true) },
            { "op_Multiply", ("*", true) },
            { "op_Division", ("/", true) },
            { "op_Append", ("+", true) },
            { "op_Concatenate", ("+", true) },
            { "op_Modulus", ("%", true) },
            { "op_BitwiseAnd", ("&", true) },
            { "op_BitwiseOr", ("|", true) },
            { "op_ExclusiveOr", ("^", true) },
            { "op_LeftShift", ("<<", true) },
            //{ "op_LogicalNot", ("!", false) },    // TODO: duplicate prefix/infix symbols
            { "op_RightShift", (">>", true) },
            //{ "op_UnaryPlus", ("+", false) },
            //{ "op_UnaryNegation", ("-", false) },
            { "op_Equality", ("==", true) },
            { "op_Inequality", ("!=", true) },
            { "op_LessThanOrEqual", ("<=", true) },
            { "op_GreaterThanOrEqual", (">=", true) },
            { "op_LessThan", ("<", true) },
            { "op_GreaterThan", (">", true) },
            { "op_PipeRight", ("|>", true) },
            { "op_PipeLeft", ("<|", true) },
            //{ "op_Dereference", ("!", false) },
            { "op_ComposeRight", (">>", true) },
            { "op_ComposeLeft", ("<<", true) },
            { "op_AdditionAssignment", ("+=", true) },
            { "op_SubtractionAssignment", ("-=", true) },
            { "op_MultiplyAssignment", ("*=", true) },
            { "op_DivisionAssignment", ("/=", true) },
        };
    }
}
