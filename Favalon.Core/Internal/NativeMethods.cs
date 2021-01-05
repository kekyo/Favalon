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

    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct PosixStat
    {
        public readonly ulong st_dev;
        public readonly ulong st_ino;
        public readonly PosixPermissions st_mode;
        private readonly uint _padding_;
        public readonly ulong st_nlink;
        public readonly uint st_uid;
        public readonly uint st_gid;
        public readonly ulong st_rdev;
        public readonly long st_size;
        public readonly long st_blksize;
        public readonly long st_blocks;
        public readonly long st_atime;
        public readonly long st_mtime;
        public readonly long st_ctime;
    }

    internal static class NativeMethods
    {
        [DllImport("libc")]
        private static extern int stat(string path, out PosixStat sb);

        public static bool IsWindows =>
            Environment.OSVersion.VersionString.Contains("Windows");
        
        public static PosixPermissions GetPosixPermissions(string path) =>
            (stat(path, out var s) == 0) ?
                s.st_mode : PosixPermissions.Nothing;
    }
}
