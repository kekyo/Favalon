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

using Favalet;
using Favalet.Contexts;
using Favalet.Expressions;
using Favalet.Internal;
using Favalon.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Favalon.Contexts
{
    public sealed class ExternalCommandRegistry :
        IVariableInformationRegistry
    {
        private static readonly char[] pathSeparators = { Path.PathSeparator };
        private static readonly char[] invalidPathChars =
            Path.GetInvalidPathChars().
            Concat(Path.GetInvalidFileNameChars()).
            Distinct().
            Memoize();
       private static readonly IExpression executor =
            CLRGenerator.Method(new Func<string, Stream, Stream>(Executor.Execute));
        
        private ExternalCommandRegistry()
        { }

        private static bool HasPosixExecutablePermissions(string path) =>
            (NativeMethods.GetPosixPermissions(path) &
                (PosixPermissions.UserExecute | PosixPermissions.GroupExecute | PosixPermissions.OtherExecute)) !=
                default;

        public (BoundAttributes attributes, ISet<VariableInformation> vis)? Lookup(string symbol)
        {
            if (symbol.IndexOfAny(invalidPathChars) == -1)
            {
                if (Environment.GetEnvironmentVariable("PATH") is { } pathVariable)
                {
                    var isWindows = NativeMethods.IsWindows;
                
                    var pattern = isWindows ? $"{symbol}.exe" : symbol;
                    Func<string, bool> filter = isWindows ? _ => true : HasPosixExecutablePermissions;

                    var candidates = new List<(int index, string path)>();
                    
                    Parallel.ForEach(
                        pathVariable.
                            Split(pathSeparators, StringSplitOptions.RemoveEmptyEntries).
                            Select((p, index) => (index, p)),
                        entry =>
                        {
                            if (Directory.Exists(entry.p))
                            {
                                foreach (var ep in Directory.EnumerateFiles(
                                    entry.p, pattern, SearchOption.TopDirectoryOnly))
                                {
                                    if (filter(ep))
                                    {
                                        lock (candidates)
                                        {
                                            candidates.Add(entry);
                                        }
                                    }
                                }
                            }
                        });

                    if (candidates.OrderBy(entry => entry.index).FirstOrDefault() is (_, { } path))
                    {
                        // Partial function application.
                        var expression = Generator.Apply(
                            executor,
                            CLRGenerator.Constant(path));
                        
                        return (BoundAttributes.PrefixLeftToRight,
                            new HashSet<VariableInformation>
                            {
                                VariableInformation.Create(symbol, executor.HigherOrder, expression)
                            });
                    }
                }
            }

            return null;
        }

        public static ExternalCommandRegistry Create() =>
            new ExternalCommandRegistry();
    }
}
