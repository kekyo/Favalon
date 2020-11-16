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
using System.Diagnostics;
using System.Reflection;

namespace Favalet.Contexts.Unifiers
{
    [DebuggerStepThrough]
    internal readonly struct UnifyResult
    {
        private static readonly Action none = () => { };

        private readonly Action? finish;

        private UnifyResult(Action? finish) =>
            this.finish = finish;

        public bool IsSucceeded =>
            this.finish != null;

        public UnifyResult Finish()
        {
            if (this.finish != null)
            {
                this.finish!();
                return Succeeded();
            }
            else
            {
                return Failed();
            }
        }

        public override string ToString() =>
            object.ReferenceEquals(this.finish, none) ?
                "Succeeded([none])" :
#if NETSTANDARD1_1
                (this.finish != null) ?
                    $"Succeeded({this.finish.GetMethodInfo().DeclaringType.Name}.{this.finish.GetMethodInfo().Name})" :
                    "Failed";
#else
                (this.finish != null) ?
                    $"Succeeded({this.finish.Method.DeclaringType!.Name}.{this.finish.Method.Name})" :
                    "Failed";
#endif

        public static UnifyResult operator &(UnifyResult lhs, UnifyResult rhs) =>
            (lhs.finish, rhs.finish) switch
            {
                // Optimization: Suppress unused finish continuation.
                (Action _, Action _) when object.ReferenceEquals(rhs.finish, none) => lhs,
                (Action _, Action _) when object.ReferenceEquals(lhs.finish, none) => rhs,

                // Combine finish continuation.
                (Action la, Action ra) => new UnifyResult(() =>
                {
                    la();
                    ra();
                }),

                _ => UnifyResult.Failed()
            };

        public static UnifyResult Succeeded() =>
            new UnifyResult(none);

        public static UnifyResult Succeeded(Action finish) =>
            new UnifyResult(finish);

        public static UnifyResult Failed() =>
            new UnifyResult(null);
    }
}
