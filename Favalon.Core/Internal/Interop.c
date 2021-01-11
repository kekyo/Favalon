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

#include <inttypes.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>

extern int32_t Favalon_Internal_NativeMethods_stat(const char* pPath, int32_t* pMode)
{
    struct stat rs;
    int32_t result = stat(pPath, &rs);
    *pMode = (int32_t)rs.st_mode;
    return result;
}
