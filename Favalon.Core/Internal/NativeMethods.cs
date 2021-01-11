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
using System.Runtime.InteropServices;

namespace Favalon.Internal
{
    [Flags]
    internal enum PosixPermissions
    {
        Nothing = 0x00,
        UserExecute = 0x40,
        GroupExecute = 0x8,
        OtherExecute = 0x1,
    }

    internal static class NativeMethods
    {
        [DllImport("Favalon.Interop", EntryPoint="Favalon_Internal_NativeMethods_stat")]
        private static extern int stat(string path, out uint mode);

        public static bool IsWindows =>
            Environment.OSVersion.VersionString.Contains("Windows");

        public static PosixPermissions GetPosixPermissions(string path)
        {
            var r = stat(path, out var mode);
            if (r == 0)
            {
                return (PosixPermissions)mode;
            }
            else
            {
                return PosixPermissions.Nothing;
            }
        }
    }
}
