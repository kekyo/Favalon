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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Favalon
{
    // TODO: test code fragments.
    public static class Test
    {
        [AliasName("echo")]
        public static IEnumerable<string> Echo(string str)
        {
            var split = str.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return split;
        }
        
        [AliasName("wc")]
        public static IEnumerable<string> WordCount(IEnumerable<string> stdin)
        {
            var bc = 0;
            var wc = 0;
            var lc = 0;

            foreach (var line in stdin)
            {
                bc += line.Length;
                wc += line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
                lc++;
            }

            yield return $"{bc},{wc},{lc}";
        }

        [AliasName("dump")]
        public static string Dump(IEnumerable<string> stdin)
        {
#if NET35
            var joined = string.Join(Environment.NewLine, stdin.ToArray());
#else
            var joined = string.Join(Environment.NewLine, stdin);
#endif
            return joined;
        }

        [AliasName("cat")]
        public static IEnumerable<string> Cat(string fileName)
        {
            using var tr = File.OpenText(fileName);
            while (!tr.EndOfStream)
            {
                var line = tr.ReadLine();
                if (line == null)
                {
                    break;
                }
                yield return line;
            }
        }
    }
}
