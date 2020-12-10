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
using System.Linq;
using System.Reflection;

namespace Favalon
{
    public static class ExceptionExtension
    {
        public static IEnumerable<string> GetReadableString(this Exception ex)
        {
            IEnumerable<string> Format(Exception ex) =>
                ex switch
                {
                    TargetInvocationException te when te.InnerException is { } ie => Format(ie),
                    AggregateException ae => ae.InnerExceptions.SelectMany(Format),
                    _ => new[] {$"{ex.GetType().Name}: {ex.Message}"}
                };

            return Format(ex);
        }
    }
}
