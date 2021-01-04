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
        public static readonly Dictionary<Type, string> ReadableTypeNames =
            new Dictionary<Type, string>
            {
                { typeof(void), "void" },
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
                { typeof(ValueTuple), "unit" },
            };
        
        public static readonly Dictionary<string, string> OperatorSymbols =
            new Dictionary<string, string>
        {
            { "op_Addition", "+" },
            { "op_Subtraction", "-" },
            { "op_Multiply", "*" },
            { "op_Division", "/" },
            { "op_Append", "+" },
            { "op_Concatenate", "+" },
            { "op_Modulus", "%" },
            { "op_BitwiseAnd", "&" },
            { "op_BitwiseOr", "|" },
            { "op_ExclusiveOr", "^" },
            { "op_LeftShift", "<<" },
            { "op_LogicalNot", "!" },
            { "op_RightShift", ">>" },
            { "op_UnaryPlus", "+" },
            { "op_UnaryNegation", "-" },
            { "op_Equality", "==" },
            { "op_Inequality", "!=" },
            { "op_LessThanOrEqual", "<=" },
            { "op_GreaterThanOrEqual", ">=" },
            { "op_LessThan", "<" },
            { "op_GreaterThan", ">" },
            { "op_PipeRight", "|>" },
            { "op_PipeLeft", "<|" },
            { "op_Dereference", "!" },
            { "op_ComposeRight", ">>" },
            { "op_ComposeLeft", "<<" },
            { "op_AdditionAssignment", "+=" },
            { "op_SubtractionAssignment", "-=" },
            { "op_MultiplyAssignment", "*=" },
            { "op_DivisionAssignment", "/=" },
        };
    }
}
