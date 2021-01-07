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

using Favalet.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Favalon.Contexts
{
    internal static class ExternalCommandExecutor
    {
        private sealed class CommandOutputStream : Stream
        {
            private readonly string path;
            private Stream? stdin;
            private Process? process;

            public CommandOutputStream(string path, Stream stdin)
            {
                this.path = path;
                this.stdin = stdin;
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    if (this.process != null)
                    {
                        this.process.Dispose();
                        this.process = null;
                    }

                    if (this.stdin != null)
                    {
                        this.stdin.Dispose();
                        this.stdin = null;
                    }
                }
            }

            private Stream? Prepare(bool raise)
            {
                if (this.process is { } p)
                {
                    return p.StandardOutput.BaseStream;
                }

                if (this.stdin is { } stdin)
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = this.path,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                    };

                    var process = Process.Start(psi)!;
                    this.process = process;

                    var thread = new Thread(parameter =>
                    {
                        var entry = (KeyValuePair<Stream, Process>)parameter!;
                        var stdin = entry.Key;
                        var processStdin = entry.Value.StandardInput.BaseStream;
                        try
                        {
                            stdin.CopyTo(processStdin);
                        }
                        finally
                        {
                            try
                            {
                                processStdin.Flush();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex);
                            }
                        }
                    });
                    
                    thread.IsBackground = true;
                    thread.Start(new KeyValuePair<Stream, Process>(stdin, process));

                    return process.StandardOutput.BaseStream;
                }

                if (raise)
                {
                    throw new InvalidOperationException();
                }
                else
                {
                    return null;
                }
            }
            
            public override bool CanRead =>
                this.Prepare(false)?.CanRead ?? false;
            public override bool CanSeek =>
                this.Prepare(false)?.CanSeek ?? false;
            public override bool CanWrite =>
                false;
            public override long Length =>
                this.Prepare(false)?.Length ?? 0;
            public override long Position
            {
                get => this.Prepare(false)?.Position ?? 0;
                set => this.Prepare(true)!.Position = value;
            }

            public override int Read(byte[] buffer, int offset, int count) =>
                this.Prepare(true)!.Read(buffer, offset, count);

            public override long Seek(long offset, SeekOrigin origin) =>
                this.Prepare(true)!.Seek(offset, origin);

            public override void SetLength(long value) =>
                this.Prepare(true)!.SetLength(value);

            public override void Write(byte[] buffer, int offset, int count) =>
                throw new InvalidOperationException();
 
            public override void Flush()
            { }
        }

        public static Stream Execute(string path, Stream stdin) =>
            new CommandOutputStream(path, stdin);
    }
}
